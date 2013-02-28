using System.Threading;

namespace Twilio.Activities
{

    /// <summary>
    /// Simple <see cref="SynchronizationContext"/> implementation that simply executes items directly.
    /// </summary>
    class SynchronizedSynchronizationContext : SynchronizationContext
    {

        public override void Post(SendOrPostCallback d, object state)
        {
            d(state);
        }

    }

}
