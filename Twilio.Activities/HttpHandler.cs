using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.DurableInstancing;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Extend and expose this HTTP handler to direct the Twilio application towards. Implement InitializeActivity to
    /// generate and return the <see cref="Activity"/> that is executed by the workflow engine. Session state is
    /// currently required to store the ongoing workflows.
    /// </summary>
    public abstract class HttpHandler : IHttpHandler, ITwilioContext
    {

        /// <summary>
        /// Query argument key name to persist the workflow instance ID.
        /// </summary>
        static readonly string InstanceIdQueryKey = "wf_InstanceId";

        /// <summary>
        /// Query argument key name to specify the bookmark to resume.
        /// </summary>
        static readonly string BookmarkQueryKey = "wf_Bookmark";

        /// <summary>
        /// Query argument key name to retrieve a resource.
        /// </summary>
        static readonly string ResourceQueryKey = "wf_Resource";

        /// <summary>
        /// Namespace under which we'll put temporary attributes.
        /// </summary>
        static readonly XNamespace ns = "http://tempuri.org/xml/Twilio.Activities";

        /// <summary>
        /// Appends the given name and value to the query string.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static Uri AppendQueryArgToUri(Uri uri, string name, string value)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (name == null)
                throw new ArgumentNullException("name");

            // parse query string and update session id
            var q = uri.Query != null ? HttpUtility.ParseQueryString(uri.Query) : new NameValueCollection();
            q[name] = value;

            // rebuild uri with new query string
            var b = new UriBuilder(uri);
            b.Query = string.Join("&", q.AllKeys.Select(i => HttpUtility.UrlEncode(i) + "=" + HttpUtility.UrlEncode(q[i])));
            return b.Uri;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public HttpHandler()
        {
            TwilioResponse = CurrentTwilioElement = new XElement("Response");
        }

        public bool IsReusable
        {
            get { return false; }
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
        /// Gets the <see cref="Activity"/> to be used to serve Twilio calls.
        /// </summary>
        public Activity Activity { get; private set; }

        /// <summary>
        /// Context into which events occuring during the execution of the workflow go.
        /// </summary>
        RunnableSynchronizationContext SynchronizationContext { get; set; }

        /// <summary>
        /// Gets the root <see cref="XElement"/> of the generated TwiML.
        /// </summary>
        public XElement TwilioResponse { get; private set; }

        /// <summary>
        /// Gets the current <see cref="XElement"/> in which to add new TwiML nodes.
        /// </summary>
        public XElement CurrentTwilioElement { get; private set; }

        /// <summary>
        /// Unhandled exception caused by execution.
        /// </summary>
        public ExceptionDispatchInfo UnhandledExceptionInfo { get; private set; }

        /// <summary>
        /// Gets the <see cref="Uri"/> of the request, excluding any query string.
        /// </summary>
        public Uri RequestUrl
        {
            get { return new Uri(Request.Url.GetLeftPart(UriPartial.Path)); }
        }

        /// <summary>
        /// Gets the relative <see cref="Uri"/> of the request.
        /// </summary>
        public Uri RelativeUrl
        {
            get { return new Uri(RequestUrl.AbsoluteUri.Remove(RequestUrl.AbsoluteUri.Length - RequestUrl.Segments.Last().Length)); }
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> to post back to and resume the workflow.
        /// </summary>
        public Uri SelfUrl
        {
            get { return LocalizeUri(RequestUrl); }
        }

        /// <summary>
        /// Appends context information to the Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Uri LocalizeUri(Uri uri)
        {
            return AppendQueryArgToUri(uri, InstanceIdQueryKey, WorkflowApplication.Id.ToString());
        }

        /// <summary>
        /// Resolves the given url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Uri ResolveUrl(string url)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            // convert relative to absolute
            if (!uri.IsAbsoluteUri)
                uri = new Uri(RelativeUrl, url);

            // url is now relative, localize
            if (uri.IsAbsoluteUri)
                uri = LocalizeUri(uri);

            // url is absolute, attempt to make relative
            if (uri.IsAbsoluteUri)
                uri = RelativeUrl.MakeRelativeUri(uri);

            return uri;
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> to post back and resume the workflow with the given bookmark.
        /// </summary>
        /// <param name="bookmarkName"></param>
        /// <returns></returns>
        public Uri ResolveBookmarkUrl(string bookmarkName)
        {
            return RelativeUrl.MakeRelativeUri(AppendQueryArgToUri(SelfUrl, BookmarkQueryKey, bookmarkName));
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> to retrieve the given resource name.
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public Uri ResolveResourceUrl(Type resourceSource, string resourceName, CultureInfo culture)
        {
            return RelativeUrl.MakeRelativeUri(AppendQueryArgToUri(RequestUrl, ResourceQueryKey,
                "T:" + resourceSource.Assembly.GetName().Name + "/" + resourceSource.FullName + "/" + resourceName + "/" + (culture ?? CultureInfo.InvariantCulture).Name));
        }

        /// <summary>
        /// Gets a reference to the loaded Workflow Application.
        /// </summary>
        WorkflowApplication WorkflowApplication { get; set; }

        /// <summary>
        /// Override this method to initialize and return the activity to be executed.
        /// </summary>
        protected abstract Activity CreateActivity();

        /// <summary>
        /// Override this method to create the instance store used to serialize workflow instances.
        /// </summary>
        /// <returns></returns>
        protected virtual InstanceStore CreateInstanceStore()
        {
            return new HttpCookieInstanceStore(Context);
        }

        /// <summary>
        /// Invoked when a web request arrives.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            // bind ourselves to the context
            Context = context;

            // request was for a resource
            if (Request[ResourceQueryKey] != null)
            {
                // handle it and return, no need to deal with workflow
                ProcessResourceRequest(Request[ResourceQueryKey]);
                return;
            }

            // obtain our activity instance
            Activity = CreateActivity();

            // generate a new instance, with arguments, if required
            if (Request[InstanceIdQueryKey] == null)
            {
                // extract arguments
                var post = GetPostData();
                var data = post
                    .AllKeys
                    .Where(i => i.StartsWith("arg_"))
                    .ToDictionary(i => i.Remove(0, 4), i => post[i]);

                // convert data to appropriate argument types
                var args = GetActivityArguments()
                    .ToDictionary(i => i.Key, i => data.ContainsKey(i.Key) ? ChangeType(data[i.Key], i.Value) : null);

                // generate new application with arguments
                WorkflowApplication = new WorkflowApplication(Activity, args);
            }
            else
                // generate new application without arguments
                WorkflowApplication = new WorkflowApplication(Activity);

            // configure application
            WorkflowApplication.Extensions.Add<ITwilioContext>(() => this);
            WorkflowApplication.SynchronizationContext = SynchronizationContext = new RunnableSynchronizationContext();
            WorkflowApplication.InstanceStore = CreateInstanceStore();
            WorkflowApplication.Aborted = OnAborted;
            WorkflowApplication.Completed = OnCompleted;
            WorkflowApplication.Idle = OnIdle;
            WorkflowApplication.OnUnhandledException = OnUnhandledException;
            WorkflowApplication.PersistableIdle = OnPersistableIdle;
            WorkflowApplication.Unloaded = OnUnloaded;

            // attempt to resolve current instance id and reload workflow state
            if (Request[InstanceIdQueryKey] != null)
                WorkflowApplication.Load(Guid.Parse(Request[InstanceIdQueryKey]));

            // postback to resume a bookmark
            if (Request[BookmarkQueryKey] != null)
                WorkflowApplication.BeginResumeBookmark(Request[BookmarkQueryKey], GetPostData(), i => WorkflowApplication.EndResumeBookmark(i), null);

            // begin running the application
            WorkflowApplication.BeginRun(i => WorkflowApplication.EndRun(i), null);

            // process any outstanding events until completion and ensure persisted
            SynchronizationContext.Run();

            // throw exception
            if (UnhandledExceptionInfo != null)
                UnhandledExceptionInfo.Throw();

            // strip off temporary attributes
            foreach (var element in TwilioResponse.DescendantsAndSelf())
                foreach (var attribute in element.Attributes())
                    if (attribute.Name.Namespace == ns)
                        attribute.Remove();

            // write finished twilio output
            Response.ContentType = "text/xml";
            using (var wrt = XmlWriter.Create(Response.Output))
                TwilioResponse.WriteTo(wrt);
        }

        /// <summary>
        /// Gets the arguments available as input to the activity.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, Type> GetActivityArguments()
        {
            return TypeDescriptor.GetProperties(Activity)
                .Cast<PropertyDescriptor>()
                .Where(i => i.PropertyType.IsGenericType)
                .Where(i =>
                    i.PropertyType.GetGenericTypeDefinition() == typeof(InArgument<>) ||
                    i.PropertyType.GetGenericTypeDefinition() == typeof(InOutArgument<>))
                .ToDictionary(i => i.Name, i => i.PropertyType.GetGenericArguments()[0]);
        }

        /// <summary>
        /// Converts the string value to the given argument type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object ChangeType(string value, Type type)
        {
            var tc = TypeDescriptor.GetConverter(type);
            if (tc == null)
                throw new NullReferenceException("Could not convert argument to appropriate type. No type descriptor.");

            return tc.ConvertFromString(value);
        }

        void OnAborted(WorkflowApplicationAbortedEventArgs args)
        {
            if (args.Reason != null)
                UnhandledExceptionInfo = ExceptionDispatchInfo.Capture(args.Reason);

            // end event loop
            SynchronizationContext.Complete();
        }

        void OnCompleted(WorkflowApplicationCompletedEventArgs args)
        {
            // we're done, hang up on the user
            TwilioResponse.Add(new XElement("Hangup"));
        }

        void OnIdle(WorkflowApplicationIdleEventArgs args)
        {
            // if the response is empty, redirect back to ourselves periodically
            if (!TwilioResponse.HasElements)
            {
                TwilioResponse.Add(new XElement("Pause", new XAttribute("length", 2)));
                TwilioResponse.Add(new XElement("Redirect", SelfUrl));
            }
        }

        UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args)
        {
            // save unhandled exception to be thrown
            if (args.UnhandledException != null)
                UnhandledExceptionInfo = ExceptionDispatchInfo.Capture(args.UnhandledException);

            // abort workflow
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

        /// <summary>
        /// Extracts the data to be passed to a bookmark.
        /// </summary>
        /// <returns></returns>
        NameValueCollection GetPostData()
        {
            // combine query string and form variables
            var q = new NameValueCollection(Request.QueryString);
            foreach (var i in Request.Form.AllKeys)
                q[i] = Request.Form[i];
            return q;
        }

        /// <summary>
        /// Parses a call status string into the proper type.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        DialCallStatus ParseCallStatus(string s)
        {
            switch (s)
            {
                case "completed":
                    return DialCallStatus.Completed;
                case "busy":
                    return DialCallStatus.Busy;
                case "no-answer":
                    return DialCallStatus.NoAnswer;
                case "failed":
                    return DialCallStatus.Failed;
                case "canceled":
                    return DialCallStatus.Canceled;
                default:
                    throw new FormatException();
            }
        }

        CallContext ITwilioContext.CreateCallContext()
        {
            return CreateCallContext();
        }

        /// <summary>
        /// Generates a new <see cref="CallContext"/> based on the current request.
        /// </summary>
        /// <returns></returns>
        CallContext CreateCallContext()
        {
            // obtain post data of current request
            var d = GetPostData();

            return new CallContext(
                d["AccountSid"],
                d["Sid"],
                ParseDirection(d["Direction"]),
                new CallEndpoint(
                    d["Caller"],
                    d["CallerCity"],
                    d["CallerCountry"],
                    d["CallerState"],
                    d["CallerZip"]),
                new CallEndpoint(
                    d["Called"],
                    d["CalledCity"],
                    d["CalledCountry"],
                    d["CalledState"],
                    d["CalledZip"]),
                new CallEndpoint(
                    d["From"],
                    d["FromCity"],
                    d["FromCountry"],
                    d["FromState"],
                    d["FromZip"]),
                new CallEndpoint(
                    d["To"],
                    d["ToCity"],
                    d["ToCountry"],
                    d["ToState"],
                    d["ToZip"]),
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses the call direction string into a <see cref="CallDirection"/> enumeration.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        CallDirection ParseDirection(string direction)
        {
            switch (direction)
            {
                case null:
                    return CallDirection.Unknown;
                case "inbound":
                    return CallDirection.Inbound;
                case "outbound":
                    return CallDirection.Outbound;
                default:
                    throw new FormatException("Unknown direction.");
            }
        }

        /// <summary>
        /// Handles a request for a resource.
        /// </summary>
        /// <param name="resourceInfo"></param>
        void ProcessResourceRequest(string resourceInfo)
        {
            ResourceManager resourceManager = null;
            string resourceName = null;
            CultureInfo resourceCulture = null;

            if (resourceInfo.StartsWith("T:"))
            {
                var s = resourceInfo.Remove(0, 2).Split('/');
                if (s.Length != 4)
                    throw new FormatException("Resource specification is invalid.");

                var a = Assembly.Load(s[0]);
                if (a == null)
                    throw new NullReferenceException("Could not load specified assembly.");

                var t = a.GetType(s[1]);
                if (t == null)
                    throw new NullReferenceException("Could not load specified type.");

                var r = s[2];
                if (string.IsNullOrWhiteSpace(r))
                    throw new FormatException("Unspecified resource name.");

                var c = s[3];
                if (c == null)
                    throw new FormatException("Unspecified culture.");

                // initialize resource settings
                resourceManager = new ResourceManager(t);
                resourceName = r;
                resourceCulture = CultureInfo.GetCultureInfo(c);
            }

            if (resourceManager == null)
                throw new NullReferenceException("Could not build ResourceManager.");
            if (resourceName == null)
                throw new NullReferenceException("Could not determine resource.");

            var o = resourceManager.GetStream(resourceName, resourceCulture);
            if (o == null)
                throw new NullReferenceException("Unknown resource name.");

            // write to memory stream
            var f = new MemoryStream();
            o.CopyTo(f);
            f.Position = 0;

            // probe data to determine format
            var d = f.ToArray();
            if (BitConverter.ToInt32(d, 0) == 1179011410 &&
                BitConverter.ToInt32(d, 8) == 1163280727)
                Response.ContentType = "audio/wav";

            // write all output
            Response.Clear();
            Response.BinaryWrite(d);
            Response.End();
        }

        Uri ITwilioContext.SelfUrl
        {
            get { return SelfUrl; }
        }

        Uri ITwilioContext.ResolveUrl(string url)
        {
            return ResolveUrl(url);
        }

        Uri ITwilioContext.ResolveBookmarkUrl(string bookmarkName)
        {
            return ResolveBookmarkUrl(bookmarkName);
        }

        Uri ITwilioContext.ResolveResourceUrl(Type resourceSource, string resourceName)
        {
            return ResolveResourceUrl(resourceSource, resourceName, null);
        }

        Uri ITwilioContext.ResolveResourceUrl(Type resourceSource, string resourceName, CultureInfo culture)
        {
            return ResolveResourceUrl(resourceSource, resourceName, culture);
        }

        XElement ITwilioContext.GetElement(NativeActivityContext context)
        {
            // look up current element scope
            var id = (Guid?)context.Properties.Find("Twilio.Activities_ScopeElementId");
            if (id == null)
                return TwilioResponse;

            // resolve element at scope, or simply return root
            return TwilioResponse.DescendantsAndSelf()
                .FirstOrDefault(i => (Guid?)i.Attribute(ns + "id") == id) ?? TwilioResponse;
        }

        void ITwilioContext.SetElement(NativeActivityContext context, XElement element)
        {
            // obtain existing or new id
            var id = (Guid)((Guid?)element.Attribute(ns + "id") ?? Guid.NewGuid());
            element.SetAttributeValue(ns + "id", id);

            // set as current scope
            context.Properties.Add("Twilio.Activities_ScopeElementId", id);
        }

    }

}