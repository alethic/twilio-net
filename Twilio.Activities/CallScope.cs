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
    public class CallScope : TwilioActivity
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public CallScope()
        {
            Constraints.Add(MustNotBeInsideOfCallScope());
        }

        /// <summary>
        /// Validates whether the DialNoun activity is contained within a Dial activity.
        /// </summary>
        /// <returns></returns>
        Constraint<CallScope> MustNotBeInsideOfCallScope()
        {
            var activityBeingValidated = new DelegateInArgument<CallScope>();
            var validationContext = new DelegateInArgument<ValidationContext>();
            var parent = new DelegateInArgument<Activity>();
            var parentIsCallScope = new Variable<bool>(env => true);

            return new Constraint<CallScope>()
            {
                Body = new ActivityAction<CallScope, ValidationContext>()
                {
                    Argument1 = activityBeingValidated,
                    Argument2 = validationContext,

                    Handler = new Sequence()
                    {
                        Variables = 
                        {
                            parentIsCallScope,
                        },
                        Activities =
                        {
                            new ForEach<Activity>()
                            {
                                Values = new GetParentChain()
                                { 
                                    ValidationContext = validationContext,
                                },
                                Body = new ActivityAction<Activity>()
                                {
                                    Argument = parent,
                                    Handler = new If(env => parent.Get(env).GetType() == typeof(CallScope))
                                    {
                                        Then = new Assign<bool>()
                                        { 
                                            To = parentIsCallScope,
                                            Value = false,
                                        },
                                    },
                                },
                            },
                            new AssertValidation()
                            {
                                Assertion = parentIsCallScope,
                                Message = "CallScope cannot be nested inside another CallScope",
                                IsWarning = false,
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Body to execute.
        /// </summary>
        public ActivityAction<CallContext> Body { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            if (Body != null)
                context.ScheduleAction(Body, context.GetExtension<ITwilioContext>().CallContext);
        }

    }

}
