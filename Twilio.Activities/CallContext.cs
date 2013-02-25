using System.Globalization;
using System.Runtime.Serialization;

namespace Twilio.Activities
{

    /// <summary>
    /// Provides information related to the ongoing Twilio call.
    /// </summary>
    [DataContract]
    public sealed class CallContext
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="accountSid"></param>
        /// <param name="sid"></param>
        /// <param name="direction"></param>
        /// <param name="caller"></param>
        /// <param name="called"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="culture"></param>
        internal CallContext(string accountSid, string sid, CallDirection direction, CallEndpoint caller, CallEndpoint called, CallEndpoint from, CallEndpoint to, CultureInfo culture)
        {
            AccountSid = accountSid;
            Sid = sid;
            Direction = direction;
            Caller = caller;
            Called = called;
            From = from;
            To = to;
            Culture = culture;
        }

        /// <summary>
        /// Sid of account handling call.
        /// </summary>
        [DataMember]
        public string AccountSid { get; private set; }

        /// <summary>
        /// Sid of call.
        /// </summary>
        [DataMember]
        public string Sid { get; private set; }

        /// <summary>
        /// Direction of call.
        /// </summary>
        [DataMember]
        public CallDirection Direction { get; private set; }

        /// <summary>
        /// Gets the endpoint description of the calling party.
        /// </summary>
        [DataMember]
        public CallEndpoint Caller { get; private set; }

        /// <summary>
        /// Gets the endpoint description of the called party.
        /// </summary>
        [DataMember]
        public CallEndpoint Called { get; private set; }

        /// <summary>
        /// Gets the endpoint description of the from party.
        /// </summary>
        [DataMember]
        public CallEndpoint From { get; private set; }

        /// <summary>
        /// Gets the endpoint description of the to party.
        /// </summary>
        [DataMember]
        public CallEndpoint To { get; private set; }

        /// <summary>
        /// Gets or sets the current culture.
        /// </summary>
        [DataMember]
        public CultureInfo Culture { get; set; }

    }

}
