using System.Activities;
using System.Activities.Statements;
using System.Activities.Validation;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Instruction on how to dial.
    /// </summary>
    public abstract class DialNoun : TwilioActivity
    {

        public DialNoun()
        {
            Constraints.Add(MustBeInsideDialActivityConstraint());
        }

        /// <summary>
        /// Validates whether the DialNoun activity is contained within a Dial activity.
        /// </summary>
        /// <returns></returns>
        Constraint<DialNoun> MustBeInsideDialActivityConstraint()
        {
            var activityBeingValidated = new DelegateInArgument<DialNoun>();
            var validationContext = new DelegateInArgument<ValidationContext>();
            var parent = new DelegateInArgument<Activity>();
            var parentIsOuter = new Variable<bool>();

            return new Constraint<DialNoun>
            {
                Body = new ActivityAction<DialNoun, ValidationContext>
                {
                    Argument1 = activityBeingValidated,
                    Argument2 = validationContext,

                    Handler = new Sequence()
                    {
                        Variables = 
                        {
                            parentIsOuter,
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
                                    Handler = new If(env => parent.Get(env).GetType() == typeof(Dial))
                                    {
                                        Then = new Assign<bool>()
                                        { 
                                            To = parentIsOuter,
                                            Value = true,
                                        },
                                    },
                                },
                            },
                            new AssertValidation()
                            {
                                Assertion = parentIsOuter,
                                Message = "DialNouns must be nested inside Dial",
                                IsWarning = false,
                            },
                        },
                    },
                },
            };
        }

    }

}
