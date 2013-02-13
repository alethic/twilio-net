using System;
using System.Runtime.DurableInstancing;
using System.Web;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Serializes workflow state to the session.
    /// </summary>
    public class HttpSessionInstanceStore : HttpInstanceStore
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="httpContext"></param>
        public HttpSessionInstanceStore(HttpContext httpContext)
            : base(httpContext)
        {

        }

        protected override void SaveToContext(Guid instanceId, XDocument doc)
        {
            // store state in session
            HttpContext.Session[string.Format("WF_{0}", instanceId)] = doc;
        }

        protected override XDocument LoadFromContext(Guid instanceId)
        {
            // resolve cookie data for workflow instance
            var doc = (XDocument)HttpContext.Session[string.Format("WF_{0}", instanceId)];
            if (doc == null)
                throw new InstancePersistenceException("Could not load workflow instance from session.");

            return doc;
        }

    }

}
