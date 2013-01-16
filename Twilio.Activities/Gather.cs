using System;
using System.Activities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    [Designer(typeof(GatherDesigner))]
    public sealed class Gather : TwilioActivity
    {

        public Gather()
        {
            Element = new Variable<XElement>();
            BookmarkName = new Variable<string>();
        }

        public InArgument<TimeSpan?> Timeout { get; set; }

        public InArgument<char?> FinishOnKey { get; set; }

        public InArgument<int?> NumDigits { get; set; }

        public OutArgument<string> Digits { get; set; }

        public Activity Body { get; set; }

        Variable<XElement> Element { get; set; }

        Variable<string> BookmarkName { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(Element);
            metadata.AddImplementationVariable(BookmarkName);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var timeout = Timeout.Get(context);
            var finishOnKey = FinishOnKey.Get(context);
            var numDigits = NumDigits.Get(context);

            // name to resume
            var bookmarkName = Guid.NewGuid().ToString();
            BookmarkName.Set(context, bookmarkName);

            // append gather element
            var element = new XElement("Gather",
                new XAttribute("action", twilio.BookmarkSelfUri(bookmarkName)),
                timeout != null ? new XAttribute("timeout", ((TimeSpan)timeout).TotalSeconds) : null,
                finishOnKey != null ? new XAttribute("finishOnKey", finishOnKey) : null,
                numDigits != null ? new XAttribute("numDigits", numDigits) : null);
            Element.Set(context, element);

            // write dial element and catch redirect
            twilio.Element.Add(element);
            twilio.Element.Add(new XElement("Redirect", twilio.BookmarkSelfUri(bookmarkName)));

            if (Body != null)
            {
                // body will write into our Gather element
                twilio.Element = element;
                context.ScheduleActivity(Body, OnBodyCompletion, OnBodyFault);
            }
            else
                Wait(context);
        }

        void OnBodyCompletion(NativeActivityContext context, ActivityInstance instance)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var element = Element.Get(context);

            // switch back to parent element
            twilio.Element = element.Parent;

            Wait(context);
        }

        void OnBodyFault(NativeActivityFaultContext context, Exception e, ActivityInstance instance)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var element = Element.Get(context);

            // switch back to parent element
            twilio.Element = element.Parent;
        }

        void Wait(NativeActivityContext context)
        {
            var bookmarkName = BookmarkName.Get(context);

            // wait for incoming digits
            context.CreateBookmark(bookmarkName, OnGatherFinished);
        }

        /// <summary>
        /// Invoked when digits are available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnGatherFinished(NativeActivityContext context, Bookmark bookmark, object o)
        {
            var r = (NameValueCollection)o;
            var digits = r["Digits"] ?? "";

            Digits.Set(context, digits);
        }

    }

}
