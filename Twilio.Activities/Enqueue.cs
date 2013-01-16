using System;
using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    public sealed class Enqueue : TwilioActivity
    {

        internal static readonly string BookmarkName = "Twilio.Enqueue";

        public InArgument<TimeSpan?> Timeout { get; set; }

        public InArgument<char?> FinishOnKey { get; set; }

        public InArgument<int?> NumDigits { get; set; }

        public OutArgument<string> Digits { get; set; }

        public Activity Body { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        XElement element;

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var timeout = Timeout.Get(context);
            var finishOnKey = FinishOnKey.Get(context);
            var numDigits = NumDigits.Get(context);

            // append gather element
            twilio.Element.Add(element = new XElement("Gather",
                new XAttribute("action", twilio.BookmarkSelfUri(BookmarkName)),
                timeout != null ? new XAttribute("timeout", ((TimeSpan)timeout).TotalSeconds) : null,
                finishOnKey != null ? new XAttribute("finishOnKey", finishOnKey) : null,
                numDigits != null ? new XAttribute("numDigits", numDigits) : null));

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

            // switch back to parent element
            twilio.Element = element.Parent;

            Wait(context);
        }

        void OnBodyFault(NativeActivityFaultContext context, Exception e, ActivityInstance instance)
        {
            var twilio = context.GetExtension<ITwilioContext>();

            // switch back to parent element
            twilio.Element = element.Parent;
        }

        void Wait(NativeActivityContext context)
        {
            // wait for incoming digits
            context.CreateBookmark(BookmarkName, OnGatherFinished);
        }

        /// <summary>
        /// Invoked when digits are available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnGatherFinished(NativeActivityContext context, Bookmark bookmark, object o)
        {
            Digits.Set(context, ((string)o));
        }

    }

}
