using System.Activities;
using System.Collections.ObjectModel;
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

        public Collection<Variable> Variables { get; set; }

        protected XElement GetElement(ActivityContext context)
        {
            return context.GetExtension<ITwilioContext>().Element;
        }

        protected void SetElement(ActivityContext context, XElement element)
        {
            context.GetExtension<ITwilioContext>().Element = element;
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

        public Collection<Variable> Variables { get; set; }

        protected XElement GetElement(ActivityContext context)
        {
            return context.GetExtension<ITwilioContext>().Element;
        }

        protected void SetElement(ActivityContext context, XElement element)
        {
            context.GetExtension<ITwilioContext>().Element = element;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension<ITwilioContext>();
        }

    }

}
