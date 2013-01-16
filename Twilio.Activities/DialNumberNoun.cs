using System.Xml.Linq;

namespace Twilio.Activities
{

    public class DialNumberNoun : DialNoun
    {

        public string Number { get; set; }

        public override void WriteTo(XElement element)
        {
            element.Add(new XElement("Number", Number));
        }

    }

}
