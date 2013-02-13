using System;
using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Produces a dial body to dial a number.
    /// </summary>
    [Designer(typeof(DialNumberDesigner))]
    public class DialNumber : DialNoun
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DialNumber()
        {

        }

        /// <summary>
        /// Number to dial.
        /// </summary>
        public InArgument<string> Number { get; set; }

        /// <summary>
        /// Digits to be sent when the number is answered.
        /// </summary>
        public InArgument<string> SendDigits { get; set; }

        /// <summary>
        /// Activities to be executed for the called party before the call is connected.
        /// </summary>
        [Browsable(false)]
        public Activity Called { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var number = Number.Get(context);
            var sendDigits = SendDigits.Get(context);

            if (Called != null)
            {
                // url attribute will execute the Called activity
                var calledBookmark = Guid.NewGuid().ToString();
                context.CreateBookmark(calledBookmark, OnCalled);

                twilio.Element.Add(new XElement("Number",
                    new XAttribute("url", twilio.BookmarkSelfUri(calledBookmark)),
                    !string.IsNullOrWhiteSpace(sendDigits) ? new XAttribute("sendDigits", sendDigits) : null,
                    number));
            }
            else
                twilio.Element.Add(new XElement("Number",
                    !string.IsNullOrWhiteSpace(sendDigits) ? new XAttribute("sendDigits", sendDigits) : null,
                    number));
        }

        /// <summary>
        /// Invoked when the called party uri is requested.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="value"></param>
        void OnCalled(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.ScheduleActivity(Called, OnCalledCompleted, OnCalledFaulted);
        }

        void OnCalledCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            twilio.Element.Add(new XElement("Pause", 0));
        }

        void OnCalledFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            var twilio = faultContext.GetExtension<ITwilioContext>();
            twilio.Element.Add(new XElement("Pause", 0));
        }

    }

}
