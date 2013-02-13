using System.Xml.Linq;

namespace Twilio.Activities
{

    public class DialNumberNoun : DialNoun
    {

        /// <summary>
        /// Number to be dialed.
        /// </summary>
        public string Number { get; set; }
        
        public override void WriteTo(XElement element)
        {
            element.Add(new XElement("Number", Number));
        }

    }

}
