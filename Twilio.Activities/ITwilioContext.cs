using System;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Provides services activities can use to interact with Twilio.
    /// </summary>
    interface ITwilioContext
    {

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
        /// Gets the root Twilio response element.
        /// </summary>
        XElement Response { get; }

        /// <summary>
        /// Gets the current element into which to insert new elements.
        /// </summary>
        XElement Element { get; set; }

    }

}
