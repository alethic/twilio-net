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

        protected override void Execute(NativeActivityContext context)
        {
            var duration = Duration.Get(context);

            GetElement(context).Add(
                new XElement("Pause",
                    new XAttribute("length", (int)duration.TotalSeconds)));
        }

    }

}
