using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Twilio.Activities
{

    class RunnableSynchronizationContext : SynchronizationContext
    {

        /// <summary>
        /// Set of actions to be dispatched.
        /// </summary>
        BlockingCollection<Tuple<SendOrPostCallback, object>> actions =
            new BlockingCollection<Tuple<SendOrPostCallback, object>>();

        public override void Post(SendOrPostCallback d, object state)
        {
            actions.Add(new Tuple<SendOrPostCallback, object>(d, state));
        }

        /// <summary>
        /// Runs the actions posted to the synchronization context until completion.
        /// </summary>
        public void Run()
        {
            foreach (var item in actions.GetConsumingEnumerable())
                item.Item1(item.Item2);
        }

        /// <summary>
        /// Completes the synchronization context.
        /// </summary>
        public void Complete()
        {
            actions.CompleteAdding();
        }

    }

}
