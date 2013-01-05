using System;
using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    [Designer(typeof(RecordDesigner))]
    public sealed class Record : TwilioActivity<string>
    {

        internal static readonly string BookmarkName = "Twilio.Record";

        public InArgument<TimeSpan?> Timeout { get; set; }

        public InArgument<char?> FinishOnKey { get; set; }

        public InArgument<TimeSpan?> MaxLength { get; set; }

        public InArgument<bool?> Transcribe { get; set; }

        public InArgument<bool?> PlayBeep { get; set; }

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

            // append record element
            twilio.Element.Add(new XElement("Record",
                new XAttribute("action", twilio.BookmarkSelfUri(BookmarkName)),
                timeout != null ? new XAttribute("timeout", ((TimeSpan)timeout).TotalSeconds) : null,
                finishOnKey != null ? new XAttribute("finishOnKey", finishOnKey) : null,
                maxLength != null ? new XAttribute("maxLength", ((TimeSpan)maxLength).TotalSeconds) : null,
                transcribe != null ? new XAttribute("transcribe", (bool)transcribe ? "true" : "false") : null,
                playBeep != null ? new XAttribute("playBeep", (bool)playBeep ? "true" : "false") : null));

            Wait(context);
        }

        void Wait(NativeActivityContext context)
        {
            // wait for incoming recording
            context.CreateBookmark(BookmarkName, OnRecordFinished);
        }

        /// <summary>
        /// Invoked when digits are available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnRecordFinished(NativeActivityContext context, Bookmark bookmark, object o)
        {
            Result.Set(context, ((RecordResult)o));
        }

    }

}
