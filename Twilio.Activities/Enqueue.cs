using System;
using System.Activities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Linq;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    [Designer(typeof(EnqueueDesigner))]
    public sealed class Enqueue : TwilioActivity<EnqueueResult>
    {

        Variable<string> actionBookmarkName = new Variable<string>();

        /// <summary>
        /// Name of the queue to be enqueued into.
        /// </summary>
        public InArgument<string> Queue { get; set; }

        /// <summary>
        /// The SID of the queue.
        /// </summary>
        public OutArgument<string> Sid { get; set; }

        /// <summary>
        /// The time the call spent in the queue.
        /// </summary>
        public OutArgument<TimeSpan> Time { get; set; }

        /// <summary>
        /// Body of Enqueue.
        /// </summary>
        public ActivityAction<EnqueueStatus> Wait { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(actionBookmarkName);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var queue = Queue.Get(context);

            // we persist this so we can schedule multiple invocations of action
            actionBookmarkName.Set(context, context.CreateTwilioBookmarkName(OnAction));

            // bookmark for wait and action
            var actionUrl = twilio.ResolveBookmarkUrl(context.CreateBookmark(actionBookmarkName.Get(context), OnAction));

            var element = new XElement("Enqueue",
                new XAttribute("action", actionUrl),
                queue);
            GetElement(context).Add(
                element,
                new XElement("Redirect", actionUrl));

            // bookmark to execute Wait activity
            if (Wait != null)
                element.Add(new XAttribute("waitUrl",
                    twilio.ResolveBookmarkUrl(context.CreateTwilioBookmark(OnWait))));
        }

        /// <summary>
        /// Invoked when the wait bookmark is resumed (the wait URL is requested)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnWait(NativeActivityContext context, Bookmark bookmark, object o)
        {
            context.ScheduleAction(Wait, ExtractEnqueueStatus((NameValueCollection)o), OnWaitComplete);
        }

        /// <summary>
        /// Extracts a <see cref="EnqueueStatus"/> from the given bookmark result.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        EnqueueStatus ExtractEnqueueStatus(NameValueCollection r)
        {
            var position = r["QueuePosition"];
            var sid = r["QueueSid"];
            var time = r["QueueTime"];
            var currentQueueSize = r["CurrentQueueSize"];

            // return new queue status
            return new EnqueueStatus(
                sid,
                string.IsNullOrWhiteSpace(position) ? 0 : int.Parse(position),
                string.IsNullOrWhiteSpace(time) ? TimeSpan.Zero : TimeSpan.FromSeconds(int.Parse(time)),
                string.IsNullOrWhiteSpace(currentQueueSize) ? 0 : int.Parse(currentQueueSize));
        }

        /// <summary>
        /// Invoked when the wait bookmark is completed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="completedInstance"></param>
        void OnWaitComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (completedInstance.State != ActivityInstanceState.Executing)
                return;

            var twilio = context.GetExtension<ITwilioContext>();

            GetElement(context).Add(
                new XElement("Redirect",
                    twilio.ResolveBookmarkUrl(context.CreateTwilioBookmark(OnWait))));
        }

        void OnAction(NativeActivityContext context, Bookmark bookmark, object o)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var r = (NameValueCollection)o;
            var result = r["QueueResult"];
            var sid = r["QueueSid"];
            var time = r["QueueTime"];

            // cancel all outstanding activities
            context.RemoveAllBookmarks();
            context.CancelChildren();

            // due to some Twilio bug, actionUrl is invoked twice, rebuild bookmark if request does not specify result
            if (result == null)
            {
                // reestablish action bookmark
                context.CreateBookmark(actionBookmarkName.Get(context), OnAction);

                // no-op
                GetElement(context).Add(
                    new XElement("Pause",
                        new XAttribute("length", 0)));
            }
            else
            {
                // set result values
                if (result != null)
                    Result.Set(context, ParseResult(result));
                if (sid != null)
                    Sid.Set(context, sid);
                if (time != null)
                    Time.Set(context, TimeSpan.FromSeconds(int.Parse(time)));
            }
        }

        EnqueueResult ParseResult(string result)
        {
            switch (result)
            {
                case "bridged":
                    return EnqueueResult.Bridged;
                case "queue-full":
                    return EnqueueResult.QueueFull;
                case "redirected":
                    return EnqueueResult.Redirected;
                case "hangup":
                    return EnqueueResult.Hangup;
                case "error":
                    return EnqueueResult.Error;
                case "system-error":
                    return EnqueueResult.SystemError;
                default:
                    throw new FormatException(string.Format("Unrecognized queue result {0}.", result));
            }
        }

    }

}
