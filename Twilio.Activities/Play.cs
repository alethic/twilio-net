using System.Activities;
using System.Xml.Linq;

namespace Twilio.Activities
{

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
                twilio.ResolveUrl(url).ToString()));
        }

    }

}
