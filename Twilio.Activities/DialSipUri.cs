using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    [Designer(typeof(DialSipUriDesigner))]
    public class DialSipUri : NativeActivity
    {

        public InArgument<string> Uri { get; set; }

        public InArgument<string> UserName { get; set; }

        public InArgument<string> Password { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var uri = Uri.Get(context);
            var userName = UserName.Get(context);
            var password = Password.Get(context);

            // add Sip element
            twilio.GetElement(context).Add(
                new XElement("Sip",
                    userName != null ? new XAttribute("username", userName) : null,
                    password != null ? new XAttribute("password", password) : null,
                    uri));
        }

    }

}
