using System;
using System.Activities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Linq;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    [Designer(typeof(GatherDesigner))]
    public sealed class Gather : TwilioActivity<string>
    {

        public InArgument<TimeSpan?> Timeout { get; set; }

        public InArgument<char?> FinishOnKey { get; set; }

        public InArgument<int?> NumDigits { get; set; }

        public OutArgument<string> Digits { get; set; }

        public Activity Body { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var timeout = Timeout.Get(context);
            var finishOnKey = FinishOnKey.Get(context);
            var numDigits = NumDigits.Get(context);

            // name to resume when gather is finished
            var bookmarkName = Guid.NewGuid().ToString();
            context.CreateBookmark(bookmarkName, OnGatherCompleted);

            // append gather element
            var element = new XElement("Gather",
                new XAttribute("action", twilio.BookmarkSelfUrl(bookmarkName)),
                timeout != null ? new XAttribute("timeout", ((TimeSpan)timeout).TotalSeconds) : null,
                finishOnKey != null ? new XAttribute("finishOnKey", finishOnKey) : null,
                numDigits != null ? new XAttribute("numDigits", numDigits) : null);

            // write gather element
            GetElement(context).Add(
                element,
                new XElement("Redirect", twilio.BookmarkSelfUrl(bookmarkName)));

            if (Body != null)
            {
                SetElement(context, element);
                context.ScheduleActivity(Body, OnBodyCompletion);
            }
        }

        void OnBodyCompletion(NativeActivityContext context, ActivityInstance instance)
        {

        }

        /// <summary>
        /// Invoked when digits are available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnGatherCompleted(NativeActivityContext context, Bookmark bookmark, object o)
        {
            var r = (NameValueCollection)o;
            var digits = r["Digits"] ?? "";

            Result.Set(context, digits);
            Digits.Set(context, digits);

            context.CancelChildren();
        }

    }

}
