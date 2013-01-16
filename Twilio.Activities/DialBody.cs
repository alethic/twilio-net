using System.Xml.Linq;

namespace Twilio.Activities
{

    public abstract class DialBody
    {

        public abstract void WriteTo(XElement dialElement);

    }

}
