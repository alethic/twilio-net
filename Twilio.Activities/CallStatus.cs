namespace Twilio.Activities
{

    public enum CallStatus
    {

        /// <summary>
        /// The called party answered the call and was connected to the caller.
        /// </summary>
        Completed,

        /// <summary>
        /// Twilio received a busy signal when trying to connect to the called party.
        /// </summary>
        Busy,

        /// <summary>
        /// The called party did not pick up before the timeout period passed.
        /// </summary>
        NoAnswer,

        /// <summary>
        /// Twilio was unable to route to the given phone number. This is frequently caused by dialing a properly formatted but non-existent phone number.
        /// </summary>
        Failed,

        /// <summary>
        /// The call was canceled via the REST API before it was answered.
        /// </summary>
        Canceled,

    }

}
