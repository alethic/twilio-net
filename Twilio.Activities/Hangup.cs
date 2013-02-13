using System.Activities;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Hangs up the current call.
    /// </summary>
    public class Hangup : TwilioActivity
    {

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            twilio.Element.Add(new XElement("Hangup"));
        }

    }

}
