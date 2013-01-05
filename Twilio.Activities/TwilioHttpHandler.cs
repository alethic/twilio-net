using System;
using System.Activities;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Extend and expose this HTTP handler to direct the Twilio application towards. Implement InitializeActivity to
    /// generate and return the <see cref="Activity"/> that is executed by the workflow engine. Session state is
    /// currently required to store the ongoing workflows.
    /// </summary>
    public abstract class TwilioHttpHandler : IHttpHandler, IRequiresSessionState, ITwilioContext
    {

        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the base uri of the current request.
        /// </summary>
        public Uri BaseUri
        {
            get { return new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Request.Path.TrimEnd('/')); }
        }

        /// <summary>
        /// Gets the <see cref="HttpContext"/> for the current Twilio request.
        /// </summary>
        public HttpContext Context { get; private set; }

        /// <summary>
        /// Gets the <see cref="HttpRequest"/>.
        /// </summary>
        public HttpRequest Request
        {
            get { return Context.Request; }
        }

        /// <summary>
        /// Gets the <see cref="HttpResponse"/> for the current Twilio request.
        /// </summary>
        public HttpResponse Response
        {
            get { return Context.Response; }
        }

        /// <summary>
        /// Gets the root <see cref="XElement"/> of the generated TwiML.
        /// </summary>
        public XElement TwilioResponse { get; private set; }

        /// <summary>
        /// Gets the current <see cref="XElement"/> in which to add new TwiML nodes.
        /// </summary>
        public XElement CurrentTwilioElement { get; private set; }

        /// <summary>
        /// Gets the <see cref="Activity"/> to be used to serve Twilio calls.
        /// </summary>
        public Activity Activity { get; private set; }

        /// <summary>
        /// Context into which events occuring during the execution of the workflow go.
        /// </summary>
        RunnableSynchronizationContext SynchronizationContext { get; set; }

        /// <summary>
        /// Gets a reference to the loaded Workflow Application.
        /// </summary>
        WorkflowApplication WfApplication { get; set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TwilioHttpHandler()
        {
            TwilioResponse = CurrentTwilioElement = new XElement("Response");
        }

        /// <summary>
        /// Implement this method to initialize and return the activity to be executed.
        /// </summary>
        protected abstract Activity InitializeActivity();

        /// <summary>
        /// Gets the <see cref="Uri"/> to post back to and resume the workflow.
        /// </summary>
        public Uri SelfUrl { get; private set; }

        /// <summary>
        /// Appends the given name and value to the query string.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Uri AppendQueryToUri(Uri uri, string name, string value)
        {
            var b = new UriBuilder(uri);
            if (b.Query != null &&
                b.Query.Length > 1)
                b.Query = b.Query.Substring(1) + "&" + name + "=" + value;
            else
                b.Query = name + "=" + value;

            return b.Uri;
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> to post back and resume the workflow with the given bookmark.
        /// </summary>
        /// <param name="bookmarkName"></param>
        /// <returns></returns>
        public Uri BookmarkSelfUrl(string bookmarkName)
        {
            return AppendQueryToUri(SelfUrl, "Bookmark", bookmarkName);
        }

        /// <summary>
        /// Invoked when a web request arrives.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            // bind ourselves to the context
            Context = context;

            // obtain our activity instance
            Activity = InitializeActivity();

            // initializes workflow application and appropriate call backs
            WfApplication = new WorkflowApplication(Activity);
            WfApplication.Extensions.Add<ITwilioContext>(() => this);
            WfApplication.SynchronizationContext = SynchronizationContext = new RunnableSynchronizationContext();
            WfApplication.InstanceStore = new SessionInstanceStore(Context);
            WfApplication.Aborted = OnAborted;
            WfApplication.Completed = OnCompleted;
            WfApplication.Idle = OnIdle;
            WfApplication.OnUnhandledException = OnUnhandledException;
            WfApplication.PersistableIdle = OnPersistableIdle;
            WfApplication.Unloaded = OnUnloaded;

            // attempt to resolve current instance id and reload workflow state
            if (Request["InstanceId"] != null)
                WfApplication.Load(Guid.Parse(Request["InstanceId"]));

            // configure initial self uri
            SelfUrl = AppendQueryToUri(new Uri(BaseUri, Path.GetFileName(Request.Path)), "InstanceId", WfApplication.Id.ToString());

            // if we've been passed a gather result, resume
            var gatherResult = GetGatherResult();
            if (gatherResult != null)
                WfApplication.BeginResumeBookmark(Gather.BookmarkName, gatherResult, i => WfApplication.EndResumeBookmark(i), null);

            // if we've been passed a record result, resume
            var recordResult = GetRecordResult();
            if (recordResult != null)
                WfApplication.BeginResumeBookmark(Record.BookmarkName, recordResult, i => WfApplication.EndResumeBookmark(i), null);

            // if we've been passed a dial result, resume
            var dialResult = GetDialResult();
            if (dialResult != null)
                WfApplication.BeginResumeBookmark(Dial.BookmarkName, dialResult, i => WfApplication.EndResumeBookmark(i), null);

            // begin running the application
            WfApplication.BeginRun(i => WfApplication.EndRun(i), null);

            // process any outstanding events until completion
            SynchronizationContext.Run();

            // write finished twilio output
            context.Response.ContentType = "text/xml";
            using (var wrt = XmlWriter.Create(Response.Output))
                TwilioResponse.WriteTo(wrt);
        }

        void OnAborted(WorkflowApplicationAbortedEventArgs args)
        {
            throw args.Reason ?? new Exception();
        }

        void OnCompleted(WorkflowApplicationCompletedEventArgs args)
        {
            // we're done, hang up on the user
            TwilioResponse.Add(new XElement("Hangup"));
        }

        void OnIdle(WorkflowApplicationIdleEventArgs args)
        {
            // we need to resume this bookmark on our next post
            var bookmark = args.Bookmarks.FirstOrDefault(i => i.BookmarkName.StartsWith("Twilio."));
            if (bookmark != null)
                SelfUrl = AppendQueryToUri(SelfUrl, "Bookmark", bookmark.BookmarkName);
        }

        UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args)
        {
            return UnhandledExceptionAction.Abort;
        }

        PersistableIdleAction OnPersistableIdle(WorkflowApplicationIdleEventArgs args)
        {
            return PersistableIdleAction.Unload;
        }

        void OnUnloaded(WorkflowApplicationEventArgs args)
        {
            // redirect back to self
            TwilioResponse.Add(new XElement("Redirect", SelfUrl));

            // end event loop
            SynchronizationContext.Complete();
        }

        /// <summary>
        /// Gets the result of a Gather operation, if available.
        /// </summary>
        /// <returns></returns>
        string GetGatherResult()
        {
            if (Request["Bookmark"] != Gather.BookmarkName)
                return null;

            return Request["Digits"];
        }

        /// <summary>
        /// Gets the result of a Record operation, if available.
        /// </summary>
        /// <returns></returns>
        RecordResult GetRecordResult()
        {
            if (Request["Bookmark"] != Record.BookmarkName)
                return null;

            var recordingUrl = Request["RecordingUrl"];
            var recordingDuration = Request["RecordingDuration"];
            var digits = Request["Digits"];

            return new RecordResult()
            {
                RecordingUrl = new Uri(recordingUrl),
                Duration = recordingDuration != null ? TimeSpan.FromSeconds(int.Parse(recordingDuration)) : TimeSpan.Zero,
                Digits = digits,
            };
        }

        /// <summary>
        /// Gets the result of a Dial operation, if available.
        /// </summary>
        /// <returns></returns>
        DialResult GetDialResult()
        {
            if (Request["Bookmark"] != Dial.BookmarkName)
                return null;

            var status = Request["DialCallStatus"];
            var sid = Request["DialCallSid"];
            var duration = Request["DialCallDuration"];
            var recordingUrl = Request["RecordingUrl"];

            return new DialResult()
            {
                Status = ParseCallStatus(status),
                Sid = sid,
                Duration = duration != null ? TimeSpan.FromSeconds(int.Parse(duration)) : TimeSpan.Zero,
                RecordingUrl = recordingUrl != null ? new Uri(recordingUrl) : null,
            };
        }

        /// <summary>
        /// Parses a call status string into the proper type.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        CallStatus ParseCallStatus(string s)
        {
            switch (s)
            {
                case "completed":
                    return CallStatus.Completed;
                case "busy":
                    return CallStatus.Busy;
                case "no-answer":
                    return CallStatus.NoAnswer;
                case "failed":
                    return CallStatus.Failed;
                case "canceled":
                    return CallStatus.Canceled;
                default:
                    throw new FormatException();
            }
        }

        Uri ITwilioContext.SelfUrl
        {
            get { return SelfUrl; }
        }

        Uri ITwilioContext.BookmarkSelfUri(string bookmarkName)
        {
            return BookmarkSelfUrl(bookmarkName);
        }

        XElement ITwilioContext.Response
        {
            get { return TwilioResponse; }
        }

        XElement ITwilioContext.Element
        {
            get { return CurrentTwilioElement; }
            set { CurrentTwilioElement = value; }
        }

    }

}