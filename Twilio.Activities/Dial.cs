using System;
using System.Activities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web;
using System.Xml.Linq;

namespace Twilio.Activities
{

    [Designer(typeof(DialDesigner))]
    public sealed class Dial : TwilioActivity
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Dial()
        {
            Nouns = new Collection<DialNoun>();
        }

        /// <summary>
        /// Timeout before dial is canceled.
        /// </summary>
        public InArgument<TimeSpan?> Timeout { get; set; }

        /// <summary>
        /// Whether if the calling party pressess '*', the they are disconnected and control is returned.
        /// </summary>
        public InArgument<bool?> HangupOnStar { get; set; }

        /// <summary>
        /// Maximum amount of time that can pass before the caller is disconnected.
        /// </summary>
        public InArgument<TimeSpan?> TimeLimit { get; set; }

        /// <summary>
        /// Caller ID to dial as.
        /// </summary>
        public InArgument<string> CallerId { get; set; }

        /// <summary>
        /// Whether the call should be recorded.
        /// </summary>
        public InArgument<bool?> Record { get; set; }

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

        /// <summary>
        /// Instructions on how to dial.
        /// </summary>
        [Browsable(false)]
        public Collection<DialNoun> Nouns { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var timeout = Timeout.Get(context);
            var hangupOnStar = HangupOnStar.Get(context);
            var timeLimit = TimeLimit.Get(context);
            var callerId = CallerId.Get(context);
            var record = Record.Get(context);

            // dial completion
            var bookmarkName = Guid.NewGuid().ToString();
            context.CreateBookmark(bookmarkName, OnDialCompleted);

            // dial element
            var element = new XElement("Dial",
                new XAttribute("action", twilio.BookmarkSelfUri(bookmarkName)),
                timeout != null ? new XAttribute("timeout", ((TimeSpan)timeout).TotalSeconds) : null,
                hangupOnStar != null ? new XAttribute("hangupOnStar", (bool)hangupOnStar ? "true" : "false") : null,
                timeLimit != null ? new XAttribute("timeLimit", ((TimeSpan)timeLimit).TotalSeconds) : null,
                callerId != null ? new XAttribute("callerId", callerId) : null,
                record != null ? new XAttribute("record", (bool)record ? "true" : "false") : null);

            // write dial element and configure context so children write into it
            twilio.Element.Add(element);
            twilio.Element.Add(new XElement("Redirect", twilio.BookmarkSelfUri(bookmarkName)));
            twilio.Element = element;

            // wait for post back

            // schedule nouns (content of Dial)
            foreach (var noun in Nouns)
                context.ScheduleActivity(noun, OnNounCompleted, OnNounFaulted);
        }

        /// <summary>
        /// Invoked when one of the nouns is completed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="completedInstance"></param>
        void OnNounCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {

        }

        /// <summary>
        /// Invoked when one of the nouns faults.
        /// </summary>
        /// <param name="faultContext"></param>
        /// <param name="propagatedException"></param>
        /// <param name="propagatedFrom"></param>
        void OnNounFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {

        }

        /// <summary>
        /// Invoked when digits are available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnDialCompleted(NativeActivityContext context, Bookmark bookmark, object o)
        {
            var r = (NameValueCollection)o;
            var status = r["DialCallStatus"];
            var sid = r["DialCallSid"];
            var duration = r["DialCallDuration"];
            var recordingUrl = r["RecordingUrl"];

            // cancel all children
            context.CancelChildren();
            context.RemoveAllBookmarks();

            // dial must have fallen through
            if (status == null)
                return;

            Status.Set(context, ParseCallStatus(status));
            Sid.Set(context, sid);
            Duration.Set(context, duration != null ? TimeSpan.FromSeconds(int.Parse(duration)) : TimeSpan.Zero);
            RecordingUrl.Set(context, recordingUrl != null ? new Uri(recordingUrl) : null);
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

    }

}
