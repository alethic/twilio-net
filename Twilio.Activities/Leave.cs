using System.Activities;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Leaves a queue.
    /// </summary>
    public sealed class Leave : TwilioActivity
    {

        protected override void Execute(NativeActivityContext context)
        {
            GetElement(context).Add(new XElement("Leave"));
        }

    }

}
