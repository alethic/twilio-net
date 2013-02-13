using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Twilio.Activities
{

    public class DialSipNoun : DialNoun
    {

        public IEnumerable<DialSipUriNoun> Uris { get; set; }

        public override void WriteTo(XElement element)
        {
            element.Add(new XElement("Sip",
                Uris.Select(i => new XElement("Uri",
                    i.UserName != null ? new XAttribute("username", i.UserName) : null,
                    i.Password != null ? new XAttribute("password", i.Password) : null,
                    i.Uri))));
        }

    }

}
