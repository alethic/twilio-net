using System.Activities;
using System.Xml.Linq;

namespace Twilio.Activities
{

    public abstract class TwilioActivity : NativeActivity
    {

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
