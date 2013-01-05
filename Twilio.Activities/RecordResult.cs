using System;
using System.Runtime.Serialization;

namespace Twilio.Activities
{

    /// <summary>
    /// Result of a Record operation.
    /// </summary>
    [DataContract]
    public class RecordResult : Result
    {

        /// <summary>
        /// The URL of the recorded audio.
        /// </summary>
        [DataMember]
        public Uri RecordingUrl { get; set; }

        /// <summary>
        /// The duration of the recorded audio.
        /// </summary>
        [DataMember]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The key (if any) pressed to end the recording or 'hangup' if the caller hung up.
        /// </summary>
        [DataMember]
        public string Digits { get; set; }

    }

}
