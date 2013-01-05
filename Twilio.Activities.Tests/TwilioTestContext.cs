using System;
using System.Activities;
using System.Xml.Linq;

namespace Twilio.Activities.Tests
{

    class TwilioTestContext : ITwilioContext
    {

        WorkflowInvoker invoker;
        Uri selfUrl = new Uri("http://www.tempuri.org/wf.ashx");
        XElement response;
        XElement element;

        public TwilioTestContext(Activity activity)
        {
            // new invoker which uses ourself as the context
            invoker = new WorkflowInvoker(activity);
            invoker.Extensions.Add<ITwilioContext>(() => this);

            response = new XElement("Response");
            element = response;
        }

        public Uri SelfUrl
        {
            get { return selfUrl; }
        }

        public Uri BookmarkSelfUri(string bookmarkName)
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

        public WorkflowInvoker Invoker
        {
            get { return invoker; }
        }

    }

}
