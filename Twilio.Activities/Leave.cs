using System.Activities;
using System.Activities.Statements;
using System.Activities.Validation;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Leaves a queue.
    /// </summary>
    public sealed class Leave : TwilioActivity
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Leave()
        {
            Constraints.Add(MustBeContainedInEnqueue());
        }

        /// <summary>
        /// Validates whether the Leave activity is contained inside of an Enqueue activity.
        /// </summary>
        /// <returns></returns>
        Constraint<Leave> MustBeContainedInEnqueue()
        {
            var activityBeingValidated = new DelegateInArgument<Leave>();
            var validationContext = new DelegateInArgument<ValidationContext>();
            var outer = new DelegateInArgument<Activity>();
            var enqueueIsOuter = new Variable<bool>();

            return new Constraint<Leave>()
            {
                Body = new ActivityAction<Leave, ValidationContext>()
                {
                    Argument1 = activityBeingValidated,
                    Argument2 = validationContext,

                    Handler = new Sequence()
                    {
                        Variables = 
                        {
                            enqueueIsOuter,
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
                                    Argument = outer,
                                    Handler = new If(env => typeof(Enqueue).IsAssignableFrom(outer.Get(env).GetType()))
                                    {
                                        Then = new Assign<bool>()
                                        { 
                                            To = enqueueIsOuter,
                                            Value = true,
                                        },
                                    },
                                },
                            },
                            new AssertValidation()
                            {
                                Assertion = enqueueIsOuter,
                                Message = "Leave must be contained inside of an Enqueue.",
                                IsWarning = false,
                            },
                        },
                    },
                },
            };
        }

        protected override void Execute(NativeActivityContext context)
        {
            GetElement(context).Add(new XElement("Leave"));
        }

    }

}
