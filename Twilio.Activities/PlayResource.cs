using System;
using System.Activities;

namespace Twilio.Activities
{

    /// <summary>
    /// Plays the audio given by the .Net resource.
    /// </summary>
    public sealed class PlayResource : NativeActivity
    {

        Variable<string> playUrl = new Variable<string>();
        Activity play;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PlayResource()
        {
            play = new Play()
            {
                Url = playUrl,
            };
        }

        /// <summary>
        /// Type that provides the resource.
        /// </summary>
        public Type ResourceSource { get; set; }

        /// <summary>
        /// Name of resource to play.
        /// </summary>
        public InArgument<string> ResourceName { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(playUrl);
            metadata.AddImplementationChild(play);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var resourceSource = ResourceSource;
            var resourceName = ResourceName.Get(context);

            if (resourceSource == null)
                throw new ArgumentNullException("ResourceSource");
            if (resourceName == null)
                throw new ArgumentNullException("ResourceName");

            var url = twilio.ResolveResourceUrl(resourceSource, resourceName);
            if (url == null)
                throw new NullReferenceException("Could not resolve resource.");

            // set url on variable referenced by child
            playUrl.Set(context, url.ToString());

            // execute play
            context.ScheduleActivity(play);
        }

    }

}
