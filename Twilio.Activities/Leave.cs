using System.Activities;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Leaves a queue.
    /// </summary>
    public class Leave : NativeActivity
    {

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();

            // add leave verb
            twilio.Element.Add(new XElement("Leave"));
        }

    }

}
