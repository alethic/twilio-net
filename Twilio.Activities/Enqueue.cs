using System;
using System.Activities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    //  [Designer(typeof(EnqueueDesigner))]
    public sealed class Enqueue : TwilioActivity<EnqueueResult>
    {

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
        public ActivityAction<EnqueueStatus> Body { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var queue = Queue.Get(context);

            // bookmark names for wait and action
            var waitBookmarkName = Guid.NewGuid().ToString();
            var actionBookmarkName = Guid.NewGuid().ToString();

            // enqueue element with bookmarks to wait and action
            context.CreateBookmark(waitBookmarkName, OnWait);
            context.CreateBookmark(actionBookmarkName, OnAction);
            GetElement(context).Add(
                new XElement("Enqueue",
                    new XAttribute("waitUrl", twilio.ResolveBookmarkUrl(waitBookmarkName)),
                    new XAttribute("action", twilio.ResolveBookmarkUrl(actionBookmarkName)),
                    queue));
        }

        /// <summary>
        /// Invoked when the wait bookmark is resumed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnWait(NativeActivityContext context, Bookmark bookmark, object o)
        {
            if (Body != null)
                context.ScheduleAction(Body, ExtractEnqueueStatus((NameValueCollection)o), OnWaitComplete);
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
            var twilio = context.GetExtension<ITwilioContext>();

            // bookmark that represents entry back into wait
            var waitBookmarkName = Guid.NewGuid().ToString();
            context.CreateBookmark(waitBookmarkName, OnWait);
            
            GetElement(context).Add(
                new XElement("Redirect",
                    twilio.ResolveBookmarkUrl(waitBookmarkName)));
        }

        void OnAction(NativeActivityContext context, Bookmark bookmark, object o)
        {
            var r = (NameValueCollection)o;
            var result = r["QueueResult"];
            var sid = r["QueueSid"];
            var time = r["QueueTime"];

            // cancel any children (waitUrl)
            context.RemoveAllBookmarks();
            context.CancelChildren();

            // set result values
            if (result != null)
                Result.Set(context, ParseResult(result));
            if (sid != null)
                Sid.Set(context, sid);
            if (time != null)
                Time.Set(context, TimeSpan.FromSeconds(int.Parse(time)));
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
                    return EnqueueResult.SystemError;
            }
        }

    }

}
