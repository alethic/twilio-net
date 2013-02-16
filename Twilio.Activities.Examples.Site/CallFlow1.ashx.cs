using System.Activities;
using System.Runtime.DurableInstancing;
using System.Web.SessionState;

namespace Twilio.Activities.Examples.Site
{

    public class CallFlow1 : HttpHandler, IRequiresSessionState
    {

        protected override Activity CreateActivity()
        {
            return new Twilio.Activities.Examples.CallFlow1();
        }

        protected override InstanceStore CreateInstanceStore()
        {
            return new HttpSessionInstanceStore(Context);
        }

    }

}