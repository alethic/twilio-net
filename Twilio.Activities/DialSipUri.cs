using System.Activities;
using System.ComponentModel;

namespace Twilio.Activities
{

    [Designer(typeof(DialSipUriDesigner))]
    public class DialSipUri : NativeActivity<DialSipBodyUri>
    {

        public InArgument<string> Uri { get; set; }

        public InArgument<string> UserName { get; set; }

        public InArgument<string> Password { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            Result.Set(context, new DialSipBodyUri()
            {
                Uri = Uri.Get(context),
                UserName = UserName.Get(context),
                Password = Password.Get(context),
            });
        }

    }

}
