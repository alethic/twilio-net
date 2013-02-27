using System;
using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    [Designer(typeof(PauseDesigner))]
    public sealed class Pause : TwilioActivity
    {

        /// <summary>
        /// Amount of time to pause for.
        /// </summary>
        [RequiredArgument]
        public InArgument<TimeSpan> Duration { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var duration = Duration.Get(context);

            // dial completion
            var bookmarkName = Guid.NewGuid().ToString();
            context.CreateBookmark(bookmarkName);

            GetElement(context).Add(
                new XElement("Pause",
                    new XAttribute("length", (int)duration.TotalSeconds)),
                new XElement("Redirect",
                    twilio.ResolveBookmarkUrl(bookmarkName)));
        }

    }

}
