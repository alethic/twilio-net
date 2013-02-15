using System;
using System.Runtime.Serialization;

namespace Twilio.Activities
{

    /// <summary>
    /// Information related to a call's enqueue status. 
    /// </summary>
    [DataContract]
    public sealed class EnqueueStatus
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="position"></param>
        /// <param name="time"></param>
        /// <param name="currentQueueSize"></param>
        internal EnqueueStatus(string sid, int position, TimeSpan time, int currentQueueSize)
        {
            Sid = sid;
            Position = position;
            Time = time;
            CurrentQueueSize = currentQueueSize;
        }

        [DataMember]
        public string Sid { get; private set; }

        [DataMember]
        public int Position { get; private set; }

        [DataMember]
        public TimeSpan Time { get; private set; }

        [DataMember]
        public int CurrentQueueSize { get; private set; }

    }

}
