using System;
using System.Activities;
using System.ComponentModel;
using System.Globalization;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    /// <summary>
    /// Plays the audio given by the .Net resource.
    /// </summary>
    [Designer(typeof(PlayResourceDesigner))]
    public sealed class PlayResource : NativeActivity
    {

        Variable<string> playUrl = new Variable<string>();
        Variable<bool?> loop = new Variable<bool?>();
        Activity play;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PlayResource()
        {
            play = new Play()
            {
                Url = playUrl,
                Loop = loop,
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

        /// <summary>
        /// Culture to use to resolve resource.
        /// </summary>
        public InArgument<CultureInfo> Culture { get; set; }

        /// <summary>
        /// Should the audio loop?
        /// </summary>
        public InArgument<int?> Loop { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(playUrl);
            metadata.AddImplementationVariable(loop);
            metadata.AddImplementationChild(play);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var resourceSource = ResourceSource;
            var resourceName = ResourceName.Get(context);
            var culture = Culture.Get(context);

            if (resourceSource == null)
                throw new ArgumentNullException("ResourceSource");
            if (resourceName == null)
                throw new ArgumentNullException("ResourceName");

            // resolve resource url
            var url = twilio.ResolveResourceUrl(resourceSource, resourceName, culture);
            if (url == null)
                throw new NullReferenceException("Could not resolve resource.");

            // set variables referenced by child
            playUrl.Set(context, url.ToString());
            loop.Set(context, Loop.Get(context));

            // execute play
            context.ScheduleActivity(play);
        }

    }

}
