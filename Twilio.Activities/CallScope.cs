using System.Activities;
using System.Activities.Statements;
using System.Activities.Validation;
using System.ComponentModel;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    /// <summary>
    /// Represents the required outside scope of a Twilio call workflow. Provides basic call information to children.
    /// </summary>
    [Designer(typeof(CallScopeDesigner))]
    public sealed class CallScope : TwilioActivity
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public CallScope()
            : base()
        {
            DisplayName = "Call Scope";
            Constraints.Clear();
        }

        /// <summary>
        /// Body to execute.
        /// </summary>
        public ActivityAction<CallContext> Body { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            if (Body != null)
                context.ScheduleAction(Body, context.GetExtension<ITwilioContext>().CreateCallContext);
        }

    }

}
