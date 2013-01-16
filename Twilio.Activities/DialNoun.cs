using System.Xml.Linq;

namespace Twilio.Activities
{

    public abstract class DialNoun
    {

        public abstract void WriteTo(XElement dialElement);

    }

}
