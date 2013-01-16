using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;

namespace Twilio.Activities
{

    [Designer(typeof(DialSipDesigner))]
    public class DialSip : DialBodyActivity
    {

        public DialSip()
        {
            Uris = new List<ActivityFunc<DialSipBodyUri>>();
            Enumerator = new Variable<IEnumerator<ActivityFunc<DialSipBodyUri>>>();
            UriList = new Variable<List<DialSipBodyUri>>();
        }

        public ICollection<ActivityFunc<DialSipBodyUri>> Uris { get; set; }

        Variable<IEnumerator<ActivityFunc<DialSipBodyUri>>> Enumerator { get; set; }

        Variable<List<DialSipBodyUri>> UriList { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(Enumerator);
            metadata.AddImplementationVariable(UriList);
        }

        protected override void Execute(NativeActivityContext context)
        {
            // initialize lsit
            UriList.Set(context, new List<DialSipBodyUri>());

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
                Result.Set(context, new DialSipBody()
                {
                    Uris = UriList.Get(context),
                });
            }
        }

        void UriCallback(NativeActivityContext context, ActivityInstance activityInstance, DialSipBodyUri uri)
        {
            // append resulting uri
            UriList.Get(context).Add(uri);

            // evaluate next func
            MoveNext(context);
        }

    }

}
