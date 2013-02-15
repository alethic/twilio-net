using System;
using System.Activities;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Provides services activities can use to interact with Twilio.
    /// </summary>
    interface ITwilioContext
    {

        /// <summary>
        /// Gets information related to the ongoing call.
        /// </summary>
        CallContext CallContext { get; }

        /// <summary>
        /// Gets the <see cref="Uri"/> to be used to submit back to the current Twilio handler.
        /// </summary>
        Uri SelfUrl { get; }

        /// <summary>
        /// Resolve the given relative url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Uri ResolveUrl(string url);

        /// <summary>
        /// Gets the <see cref="Uri"/> to be used to submit back to the current Twilio handler, with the given bookmark.
        /// </summary>
        /// <param name="bookmarkName"></param>
        /// <returns></returns>
        Uri BookmarkSelfUrl(string bookmarkName);

        /// <summary>
        /// Gets the current element based on the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        XElement GetElement(NativeActivityContext context);

        /// <summary>
        /// Sets the current element for the context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="element"></param>
        void SetElement(NativeActivityContext context, XElement element);

    }

}
