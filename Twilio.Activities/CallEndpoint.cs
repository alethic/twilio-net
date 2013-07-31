using System.Runtime.Serialization;

namespace Twilio.Activities
{

    /// <summary>
    /// Describes an endpoint participating in a call.
    /// </summary>
    [DataContract]
    public sealed class CallEndpoint
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <param name="city"></param>
        /// <param name="country"></param>
        /// <param name="state"></param>
        /// <param name="zip"></param>
        internal CallEndpoint(string target, string name, string city, string country, string state, string zip)
        {
            Target = target;
            Name = name;
            City = city;
            Country = country;
            State = state;
            Zip = zip;
        }

        /// <summary>
        /// Number of endpoint.
        /// </summary>
        [DataMember]
        public string Target { get; private set; }

        /// <summary>
        /// Name of endpoint.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// City of endpoint.
        /// </summary>
        [DataMember]
        public string City { get; private set; }

        /// <summary>
        /// Country of endpoint.
        /// </summary>
        [DataMember]
        public string Country { get; private set; }

        /// <summary>
        /// State of endpoint.
        /// </summary>
        [DataMember]
        public string State { get; private set; }

        /// <summary>
        /// Zip of endpoint.
        /// </summary>
        [DataMember]
        public string Zip { get; private set; }

    }

}
