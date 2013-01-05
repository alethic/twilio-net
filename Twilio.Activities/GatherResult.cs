using System.Runtime.Serialization;

namespace Twilio.Activities
{

    /// <summary>
    /// Results from a Gather operation.
    /// </summary>
    [DataContract]
    public class GatherResult : Result
    {

        /// <summary>
        /// Digits that were gathered.
        /// </summary>
        [DataMember]
        public string Digits { get; set; }

    }

}
