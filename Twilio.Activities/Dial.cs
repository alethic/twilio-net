using System;
using System.Activities;
using System.Xml.Linq;

namespace Twilio.Activities
{

    public sealed class Dial : TwilioActivity
    {

        internal static readonly string BookmarkName = "Twilio.Dial";

        public InArgument<TimeSpan?> Timeout { get; set; }

        public InArgument<bool?> HangupOnStar { get; set; }

        public InArgument<TimeSpan?> TimeLimit { get; set; }

        public InArgument<string> CallerId { get; set; }

        public InArgument<bool?> Record { get; set; }

        public InArgument<string> Number { get; set; }

        /// <summary>
        /// The outcome of the dial attempt.
        /// </summary>
        public OutArgument<CallStatus> Status { get; set; }

        /// <summary>
        /// The call sid of the new call leg. This parameter is not sent after dialing a conference.
        /// </summary>
        public OutArgument<string> Sid { get; set; }

        /// <summary>
        /// The duration of the dialed call. This parameter is not sent after dialing a conference.
        /// </summary>
        public OutArgument<TimeSpan> Duration { get; set; }

        /// <summary>
        /// The URL of the recorded audio.
        /// </summary>
        public OutArgument<Uri> RecordingUrl { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        XElement element;

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var timeout = Timeout.Get(context);
            var hangupOnStar = HangupOnStar.Get(context);
            var timeLimit = TimeLimit.Get(context);
            var callerId = CallerId.Get(context);
            var record = Record.Get(context);
            var number = Number.Get(context);

            // append gather element
            twilio.Element.Add(element = new XElement("Dial",
                new XAttribute("action", twilio.BookmarkSelfUri(BookmarkName)),
                timeout != null ? new XAttribute("timeout", ((TimeSpan)timeout).TotalSeconds) : null,
                hangupOnStar != null ? new XAttribute("hangupOnStar", (bool)hangupOnStar ? "true" : "false") : null,
                timeLimit != null ? new XAttribute("timeLimit", ((TimeSpan)timeLimit).TotalSeconds) : null,
                callerId != null ? new XAttribute("callerId", callerId) : null,
                record != null ? new XAttribute("record", (bool)record ? "true" : "false") : null,
                number != null ? new XElement("Number", number) : null));

            Wait(context);
        }

        void Wait(NativeActivityContext context)
        {
            // wait for incoming digits
            context.CreateBookmark(BookmarkName, OnDialResult);
        }

        /// <summary>
        /// Invoked when digits are available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnDialResult(NativeActivityContext context, Bookmark bookmark, object o)
        {
            var r = (DialResult)o;
            Status.Set(context, r.Status);
            Sid.Set(context, r.Sid);
            Duration.Set(context, r.Duration);
            RecordingUrl.Set(context, r.RecordingUrl);
        }

    }

}
