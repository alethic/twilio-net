using System;
using System.Activities;
using System.Linq;
using System.Xml.Linq;

namespace Twilio.Activities.Tests
{

    class TwilioTestContext : ITwilioContext
    {

        /// <summary>
        /// Namespace under which we'll put temporary attributes.
        /// </summary>
        static readonly XNamespace ns = "http://tempuri.org/xml/Twilio.Activities";

        RunnableSynchronizationContext sync;
        WorkflowApplication app;
        Uri selfUrl = new Uri("http://www.tempuri.org/wf.ashx");
        XElement response;
        XElement element;

        public TwilioTestContext(Activity activity)
        {
            sync = new RunnableSynchronizationContext();

            // new invoker which uses ourself as the context
            app = new WorkflowApplication(activity);
            app.Extensions.Add<ITwilioContext>(() => this);

            response = new XElement("Response");
            element = response;
        }

        public void Invoke()
        {
            app.Run();
        }

        public Uri SelfUrl
        {
            get { return selfUrl; }
        }

        public Uri ResolveUrl(string url)
        {
            var uri = new Uri(url);
            if (uri.IsAbsoluteUri)
                return uri;
            else
                return new Uri(SelfUrl, url);
        }

        public Uri BookmarkSelfUrl(string bookmarkName)
        {
            return new Uri(SelfUrl, "?B=R");
        }

        public XElement Response
        {
            get { return response; }
        }

        public XElement Element
        {
            get { return element; }
            set { element = value; }
        }

        public CallContext CallContext
        {
            get
            {
                return new CallContext(
                    "TEST",
                    "TEST",
                    CallDirection.Unknown,
                    new CallEndpoint("TEST", "TEST", "TEST", "TEST", "TEST"),
                    new CallEndpoint("TEST", "TEST", "TEST", "TEST", "TEST"),
                    new CallEndpoint("TEST", "TEST", "TEST", "TEST", "TEST"),
                    new CallEndpoint("TEST", "TEST", "TEST", "TEST", "TEST"));
            }
        }

        public XElement GetElement(NativeActivityContext context)
        {
            // look up current element scope
            var id = (Guid?)context.Properties.Find("Twilio.Activities_ScopeElementId");
            if (id == null)
                return response;

            // resolve element at scope, or simply return root
            return response.DescendantsAndSelf()
                .FirstOrDefault(i => (Guid?)i.Attribute(ns + "id") == id) ?? response;
        }

        public void SetElement(NativeActivityContext context, XElement element)
        {
            // obtain existing or new id
            var id = (Guid)((Guid?)element.Attribute(ns + "id") ?? Guid.NewGuid());
            element.SetAttributeValue(ns + "id", id);

            // set as current scope
            context.Properties.Add("Twilio.Activities_ScopeElementId", id);
        }

    }

}
