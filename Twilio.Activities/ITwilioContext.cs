using System;
using System.Activities;
using System.Globalization;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Provides services activities can use to interact with Twilio.
    /// </summary>
    public interface ITwilioContext
    {

        /// <summary>
        /// Gets the <see cref="Uri"/> to be used to submit back to the current Twilio handler.
        /// </summary>
        Uri SelfUrl { get; }

        /// <summary>
        /// Gets information related to the ongoing call.
        /// </summary>
        CallContext GetCallContext();

        /// <summary>
        /// Resolve the given relative url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Uri ResolveUrl(string url);

        /// <summary>
        /// Gets the <see cref="Uri"/> to be used to submit back to the current Twilio handler, with the given bookmark.
        /// </summary>
        /// <param name="bookmark"></param>
        /// <returns></returns>
        Uri ResolveBookmarkUrl(Bookmark bookmark);

        /// <summary>
        /// Gets the <see cref="Uri"/> to be used to refer to the .Net resource.
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Uri ResolveResourceUrl(Type resourceSource, string resource);

        /// <summary>
        /// Gets the <see cref="Uri"/> to be used to refer to the .Net resource.
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <param name="resource"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        Uri ResolveResourceUrl(Type resourceSource, string resource, CultureInfo culture);

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
