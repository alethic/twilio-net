using System.Collections.Generic;
using System.Xml.Linq;

namespace Twilio.Activities
{

    public class DialSipNoun : DialNoun
    {

        public IEnumerable<DialSipUriNoun> Uris { get; set; }

        public override void WriteTo(XElement element)
        {
            foreach (var uri in Uris)
                element.Add(new XElement("Uri",
                    uri.UserName != null ? new XAttribute("username", uri.UserName) : null,
                    uri.Password != null ? new XAttribute("password", uri.Password) : null,
                    uri.Uri));
        }

    }

}
