using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;
using Twilio.Activities.Design;

namespace Twilio.Activities
{

    [Designer(typeof(DialSipUriDesigner))]
    public class DialSipUri : TwilioActivity
    {

        public InArgument<string> Uri { get; set; }

        public InArgument<string> UserName { get; set; }

        public InArgument<string> Password { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var uri = Uri.Get(context);
            var userName = UserName.Get(context);
            var password = Password.Get(context);

            // add Sip element
            GetElement(context).Add(
                new XElement("Uri",
                    userName != null ? new XAttribute("username", userName) : null,
                    password != null ? new XAttribute("password", password) : null,
                    uri));
        }

    }

}
