namespace Twilio.Activities
{

    /// <summary>
    /// Results of an Enqueue activity.
    /// </summary>
    public enum EnqueueResult
    {

        Bridged,
        QueueFull,
        Redirected,
        Hangup,
        Error,
        SystemError,

    }

}
