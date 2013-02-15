using System;
using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    [Designer(typeof(SayDesigner))]
    public sealed class Say : TwilioActivity
    {

        /// <summary>
        /// Text to be spoken.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Text { get; set; }

        public InArgument<Voice?> Voice { get; set; }

        public InArgument<string> Language { get; set; }

        public InArgument<int?> Loop { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var text = Text.Get(context);
            var voice = Voice.Get(context);
            var language = Language.Get(context);
            var loop = Loop.Get(context);

            GetElement(context).Add(new XElement("Say",
                voice != null ? new XAttribute("voice", VoiceToString((Voice)voice)) : null,
                language != null ? new XAttribute("language", language) : null,
                loop != null ? new XAttribute("loop", loop) : null,
                text));
        }

        string VoiceToString(Voice voice)
        {
            switch (voice)
            {
                case Activities.Voice.Man:
                    return "man";
                case Activities.Voice.Woman:
                    return "woman";
                default:
                    throw new InvalidOperationException("Unknown voice.");
            }
        }

    }

}
