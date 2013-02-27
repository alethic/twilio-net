using System;
using System.Activities;

namespace Twilio.Activities
{

    static class ActivityContextExtensions
    {

        /// <summary>
        /// Creates a bookmark name for the given callback.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static string CreateTwilioBookmarkName(this NativeActivityContext context, BookmarkCallback callback)
        {
            var activity = callback.Target as Activity;
            if (activity == null)
                throw new ArgumentException("Callback must be instance method of Activity.");

            return string.Format("{0}_{1}_{2}_{3}",
                Math.Abs(Guid.NewGuid().GetHashCode()),
                activity.Id,
                activity.DisplayName,
                callback.Method.Name);
        }

        /// <summary>
        /// Creates a bookmark name for the given activity.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static string CreateTwilioBookmarkName(this NativeActivityContext context, Activity activity)
        {
            return string.Format("{0}_{1}_{2}",
                Math.Abs(Guid.NewGuid().GetHashCode()),
                activity.Id,
                activity.DisplayName);
        }

        /// <summary>
        /// Creates a point at which an activity can be resumed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Bookmark CreateTwilioBookmark(this NativeActivityContext context, BookmarkCallback callback)
        {
            return context.CreateBookmark(CreateTwilioBookmarkName(context, callback), callback);
        }

        /// <summary>
        /// Creates a point at which an activity can be resumed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static Bookmark CreateTwilioBookmark(this NativeActivityContext context, Activity activity)
        {
            return context.CreateBookmark(CreateTwilioBookmarkName(context, activity));
        }

    }

}
