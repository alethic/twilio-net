using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        /// Gets the desired <see cref="PersistenceStorageMode"/> for persisting workflows between requests. Override
        /// this property to alter the mode. By default ASP.Net session state is used for persisting workflows.
        /// </summary>
        protected virtual PersistenceStorageMode PersistenceStorageMode
        {
            get { return PersistenceStorageMode.Session; }
        }

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
            WfApplication.InstanceStore = new TwilioHttpInstanceStore(Context, PersistenceStorageMode);
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

            // postback to resume a bookmark
            if (Request["Bookmark"] != null)
            {
                var result = GetBookmarkResult();
                if (result != null)
                    WfApplication.BeginResumeBookmark(Request["Bookmark"], result, i => WfApplication.EndResumeBookmark(i), null);
            }

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
            // redirect to ourselves, in case no idle activity has already done it
            TwilioResponse.Add(new XElement("Redirect", SelfUrl));
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
            // end event loop
            SynchronizationContext.Complete();
        }

        NameValueCollection GetBookmarkResult()
        {
            // extract all query arguments if bookmark specified
            if (Request["Bookmark"] != null)
                return Request.QueryString;

            return null;
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