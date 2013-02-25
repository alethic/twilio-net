using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    /// <summary>
    /// Plays the resource given by Url. Paths will be relative to the handler serving the Twilio workflow.
    /// </summary>
    [Designer(typeof(PlayDesigner))]
    public sealed class Play : TwilioActivity
    {

        /// <summary>
        /// Relative or absolute path of the file to be played.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Url { get; set; }

        public InArgument<int?> Loop { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var url = Url.Get(context);
            var loop = Loop.Get(context);

            GetElement(context).Add(new XElement("Play",
                loop != null ? new XAttribute("loop", loop) : null,
                url));
        }

    }

}
