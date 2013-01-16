using System.Activities;

namespace Twilio.Activities.Examples.Site
{

    public class CallFlow1 : TwilioHttpHandler
    {

        protected override Activity InitializeActivity()
        {
            return new Twilio.Activities.Examples.CallFlow1();
        }

    }

}