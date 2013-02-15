using System;
using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;
using System.Linq;

using Twilio.Activities.Design;
using System.Activities.Validation;

namespace Twilio.Activities
{

    /// <summary>
    /// Produces a dial body to dial a number.
    /// </summary>
    [Designer(typeof(DialNumberDesigner))]
    public class DialNumber : DialNoun
    {

        /// <summary>
        /// Number to dial.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Number { get; set; }

        /// <summary>
        /// Digits to be sent when the number is answered.
        /// </summary>
        public InArgument<string> SendDigits { get; set; }

        /// <summary>
        /// Activities to be executed for the called party before the call is connected.
        /// </summary>
        [Browsable(false)]
        public Activity Pickup { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void ExecuteNoun(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var number = Number.Get(context);
            var sendDigits = SendDigits.Get(context);

            // add new Number element
            var element = new XElement("Number",
                !string.IsNullOrWhiteSpace(sendDigits) ? new XAttribute("sendDigits", sendDigits) : null,
                number);
            GetElement(context).Add(element);

            // bookmark to execute Called activity
            if (Pickup != null)
            {
                var calledBookmark = Guid.NewGuid().ToString();
                context.CreateBookmark(calledBookmark, OnCalled);
                element.Add(new XAttribute("url", twilio.BookmarkSelfUrl(calledBookmark)));
            }
        }

        /// <summary>
        /// Invoked when the called party uri is requested.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="value"></param>
        void OnCalled(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.ScheduleActivity(Pickup, OnCalledCompleted);
        }

        void OnCalledCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (!GetElement(context).HasElements)
                GetElement(context).Add(new XElement("Pause",
                    new XAttribute("length", 0)));
        }

    }

}
