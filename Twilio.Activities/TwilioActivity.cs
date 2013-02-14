using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    public abstract class TwilioActivity : NativeActivity
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TwilioActivity()
        {
            Variables = new Collection<Variable>();
        }

        [Browsable(false)]
        public Collection<Variable> Variables { get; set; }

        /// <summary>
        /// Gets the current element based on the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected XElement GetElement(NativeActivityContext context)
        {
            return context.GetExtension<ITwilioContext>()
                .GetElement(context);
        }

        /// <summary>
        /// Sets the current element for the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="element"></param>
        protected void SetElement(NativeActivityContext context, XElement element)
        {
            context.GetExtension<ITwilioContext>()
                .SetElement(context, element);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension<ITwilioContext>();
        }

    }

    public abstract class TwilioActivity<TResult> : NativeActivity<TResult>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TwilioActivity()
        {
            Variables = new Collection<Variable>();
        }

        [Browsable(false)]
        public Collection<Variable> Variables { get; set; }

        /// <summary>
        /// Gets the current element based on the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected XElement GetElement(NativeActivityContext context)
        {
            return context.GetExtension<ITwilioContext>()
                .GetElement(context);
        }

        /// <summary>
        /// Sets the current element for the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="element"></param>
        protected void SetElement(NativeActivityContext context, XElement element)
        {
            context.GetExtension<ITwilioContext>()
                .SetElement(context, element);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension<ITwilioContext>();
        }

    }

}
