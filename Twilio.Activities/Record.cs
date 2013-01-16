using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    [Designer(typeof(RecordDesigner))]
    public sealed class Record : TwilioActivity
    {

        public InArgument<TimeSpan?> Timeout { get; set; }

        public InArgument<char?> FinishOnKey { get; set; }

        public InArgument<TimeSpan?> MaxLength { get; set; }

        public InArgument<bool?> Transcribe { get; set; }

        public InArgument<bool?> PlayBeep { get; set; }

        /// <summary>
        /// The URL of the recorded audio.
        /// </summary>
        public OutArgument<Uri> RecordingUrl { get; set; }

        /// <summary>
        /// The duration of the recorded audio.
        /// </summary>
        public OutArgument<TimeSpan> Duration { get; set; }

        /// <summary>
        /// The key (if any) pressed to end the recording or 'hangup' if the caller hung up.
        /// </summary>
        public OutArgument<string> Digits { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var timeout = Timeout.Get(context);
            var finishOnKey = FinishOnKey.Get(context);
            var maxLength = MaxLength.Get(context);
            var transcribe = Transcribe.Get(context);
            var playBeep = PlayBeep.Get(context);

            // name to resume
            var bookmarkName = Guid.NewGuid().ToString();

            // append record element
            var element = new XElement("Record",
                new XAttribute("action", twilio.BookmarkSelfUri(bookmarkName)),
                timeout != null ? new XAttribute("timeout", ((TimeSpan)timeout).TotalSeconds) : null,
                finishOnKey != null ? new XAttribute("finishOnKey", finishOnKey) : null,
                maxLength != null ? new XAttribute("maxLength", ((TimeSpan)maxLength).TotalSeconds) : null,
                transcribe != null ? new XAttribute("transcribe", (bool)transcribe ? "true" : "false") : null,
                playBeep != null ? new XAttribute("playBeep", (bool)playBeep ? "true" : "false") : null);

            // write dial element and catch redirect
            twilio.Element.Add(element);
            twilio.Element.Add(new XElement("Redirect", twilio.BookmarkSelfUri(bookmarkName)));

            // wait for post back
            context.CreateBookmark(bookmarkName, OnRecordFinished);
        }

        /// <summary>
        /// Invoked when digits are available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnRecordFinished(NativeActivityContext context, Bookmark bookmark, object o)
        {
            var r = (Dictionary<string, string>)o;
            var recordingUrl = r["RecordingUrl"];
            var recordingDuration = r["RecordingDuration"];
            var digits = r["Digits"];

            RecordingUrl.Set(context, new Uri(recordingUrl));
            Duration.Set(context, recordingDuration != null ? TimeSpan.FromSeconds(int.Parse(recordingDuration)) : TimeSpan.Zero);
            Digits.Set(context, digits);
        }

    }

}
