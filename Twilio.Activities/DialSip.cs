using System;
using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    /// <summary>
    /// Produces a dial body to dial a SIP address.
    /// </summary>
    [Designer(typeof(DialSipDesigner))]
    public sealed class DialSip : DialNoun
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DialSip()
        {
            Uris = new Collection<DialSipUri>();
        }

        /// <summary>
        /// SIP URIs to dial.
        /// </summary>
        [Browsable(false)]
        public Collection<DialSipUri> Uris { get; set; }

        /// <summary>
        /// Activities to be executed for the called party before the call is connected.
        /// </summary>
        [Browsable(false)]
        public Activity Called { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void ExecuteNoun(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();

            // insert Sip element
            var element = new XElement("Sip");
            GetElement(context).Add(element);

            if (Called != null)
                // url attribute will execute the Called activity
                element.Add(new XAttribute("url",
                    twilio.ResolveBookmarkUrl(context.CreateTwilioBookmark(OnCalled))));

            // schedule URI activities
            if (Uris.Count > 0)
            {
                twilio.SetElement(context, element);
                foreach (var uri in Uris)
                    context.ScheduleActivity(uri);
            }
        }

        /// <summary>
        /// Invoked when the called party uri is requested.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="value"></param>
        void OnCalled(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.ScheduleActivity(Called, OnCalledCompleted);
        }

        /// <summary>
        /// Invoked when the body for the called party URI is completed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="completedInstance"></param>
        void OnCalledCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (completedInstance.State != ActivityInstanceState.Executing)
                return;

            GetElement(context).Add(
                new XElement("Pause",
                    new XAttribute("length", 0)));
        }

    }

}
