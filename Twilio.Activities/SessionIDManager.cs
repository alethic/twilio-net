using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace Twilio.Activities
{

    /// <summary>
    /// Manages SessionIDs for Twilio. Twilio does not support moving cookies between call contexts such as the called
    /// party portion of a Dial verb. This manager requires the SessionID to be specified on the QueryString itself.
    /// </summary>
    public class SessionIDManager : ISessionIDManager
    {

        /// <summary>
        /// Query argument key name to persist the session ID.
        /// </summary>
        public static readonly string SessionIDQueryKey = "s_SessionId";

        /// <summary>
        /// Applies the SessionID to the given Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri ApplySessionIDQueryArg(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            var ctx = HttpContext.Current;
            if (ctx == null)
                return uri;

            if (ctx.Session == null)
                return uri;

            // parse query string and update session id
            var q = uri.IsAbsoluteUri ? HttpUtility.ParseQueryString(uri.Query) : new NameValueCollection();
            q[SessionIDQueryKey] = ctx.Session.SessionID;

            // rebuild uri with new query string
            var b = new UriBuilder(uri);
            b.Query = string.Join("&", q.AllKeys.Select(i => HttpUtility.UrlEncode(i) + "=" + HttpUtility.UrlEncode(q[i])));
            return b.Uri;
        }

        public void Initialize()
        {

        }

        public bool InitializeRequest(HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue)
        {
            supportSessionIDReissue = false;
            return context.Response.IsRequestBeingRedirected;
        }

        public string CreateSessionID(HttpContext context)
        {
            return Guid.NewGuid().ToString();
        }

        public void SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded)
        {
            redirected = false;
            cookieAdded = false;
        }

        public string GetSessionID(HttpContext context)
        {
            return context.Request.QueryString[SessionIDQueryKey];
        }

        public void RemoveSessionID(HttpContext context)
        {

        }

        public bool Validate(string id)
        {
            return true;
        }

    }

}
