using System.Activities;

namespace Twilio.Activities.Tests
{

    public abstract class TwilioTest
    {

        /// <summary>
        /// Creates a new context for execution of a Twilio workflow activitiy.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        internal TwilioTestContext CreateContext(Activity activity)
        {
            return new TwilioTestContext(activity);
        }

    }

}
