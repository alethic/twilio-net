using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;

namespace Twilio.Activities
{

    [Designer(typeof(DialSipDesigner))]
    public class DialSip : DialNoun
    {

        public DialSip()
        {
            Uris = new List<ActivityFunc<DialSipUriNoun>>();
            Enumerator = new Variable<IEnumerator<ActivityFunc<DialSipUriNoun>>>();
            UriList = new Variable<List<DialSipUriNoun>>();
        }

        public ICollection<ActivityFunc<DialSipUriNoun>> Uris { get; set; }

        Variable<IEnumerator<ActivityFunc<DialSipUriNoun>>> Enumerator { get; set; }

        Variable<List<DialSipUriNoun>> UriList { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(Enumerator);
            metadata.AddImplementationVariable(UriList);
        }

        protected override void Execute(NativeActivityContext context)
        {
            // initialize lsit
            UriList.Set(context, new List<DialSipUriNoun>());

            // begin evaluating funcs
            Enumerator.Set(context, Uris.GetEnumerator());

            // evaluate next func
            MoveNext(context);
        }

        /// <summary>
        /// Schedules the next func, or exits
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        void MoveNext(NativeActivityContext context)
        {
            var i = Enumerator.Get(context);
            if (i.MoveNext())
                context.ScheduleFunc(i.Current, UriCallback);
            else
            {
                Result.Set(context, new DialSipNoun()
                {
                    Uris = UriList.Get(context),
                });
            }
        }

        void UriCallback(NativeActivityContext context, ActivityInstance activityInstance, DialSipUriNoun uri)
        {
            // append resulting uri
            UriList.Get(context).Add(uri);

            // evaluate next func
            MoveNext(context);
        }

    }

}
