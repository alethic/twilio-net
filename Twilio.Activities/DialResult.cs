using System;
using System.Runtime.Serialization;

namespace Twilio.Activities
{

    class DialResult : Result
    {

        /// <summary>
        /// The outcome of the dial attempt.
        /// </summary>
        public CallStatus Status { get; set; }

        /// <summary>
        /// The call sid of the new call leg. This parameter is not sent after dialing a conference.
        /// </summary>
        public string Sid { get; set; }

        /// <summary>
        /// The duration of the dialed call. This parameter is not sent after dialing a conference.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The URL of the recorded audio.
        /// </summary>
        public Uri RecordingUrl { get; set; }

    }

}
