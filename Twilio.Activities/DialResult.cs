using System;
using System.Runtime.Serialization;

namespace Twilio.Activities
{

    [DataContract]
    class DialResult : Result
    {

        /// <summary>
        /// The outcome of the dial attempt.
        /// </summary>
        [DataMember]
        public CallStatus Status { get; set; }

        /// <summary>
        /// The call sid of the new call leg. This parameter is not sent after dialing a conference.
        /// </summary>
        [DataMember]
        public string Sid { get; set; }

        /// <summary>
        /// The duration of the dialed call. This parameter is not sent after dialing a conference.
        /// </summary>
        [DataMember]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The URL of the recorded audio.
        /// </summary>
        [DataMember]
        public Uri RecordingUrl { get; set; }

    }

}
