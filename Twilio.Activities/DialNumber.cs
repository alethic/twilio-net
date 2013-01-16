using System.Activities;
using System.ComponentModel;

namespace Twilio.Activities
{

    /// <summary>
    /// Produces a dial body to dial a number.
    /// </summary>
    [Designer(typeof(DialNumberDesigner))]
    public class DialNumber : DialNounActivity
    {

        public InArgument<string> Number { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            Result.Set(context, new DialNumberNoun()
            {
                Number = Number.Get(context),
            });
        }

    }

}
