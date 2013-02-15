using System.Activities;
using System.Activities.Statements;
using System.Activities.Validation;
using System.ComponentModel;
using System.Xml.Linq;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    [Designer(typeof(DialSipUriDesigner))]
    public sealed class DialSipUri : TwilioActivity
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DialSipUri()
        {
            Constraints.Add(MustBeInsideDialSipActivityConstraint());
        }

        public InArgument<string> Uri { get; set; }

        public InArgument<string> UserName { get; set; }

        public InArgument<string> Password { get; set; }

        Constraint<DialSipUri> MustBeInsideDialSipActivityConstraint()
        {
            var activityBeingValidated = new DelegateInArgument<DialSipUri>();
            var validationContext = new DelegateInArgument<ValidationContext>();
            var parent = new DelegateInArgument<Activity>();
            var parentIsOuter = new Variable<bool>();

            return new Constraint<DialSipUri>()
            {
                Body = new ActivityAction<DialSipUri, ValidationContext>()
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
                                    Handler = new If()
                                    {
                                        Condition = new InArgument<bool>(env => 
                                            parent.Get(env).GetType() == typeof(DialSip)),

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
                                Message = "DialSipUris must be nested inside DialSip",
                                IsWarning = false,
                            },
                        },
                    },
                },
            };
        }

        protected override void Execute(NativeActivityContext context)
        {
            var uri = Uri.Get(context);
            var userName = UserName.Get(context);
            var password = Password.Get(context);

            if (GetElement(context).Name != "Sip")
                throw new InvalidWorkflowException("DialSipUri executing without Sip element. All DialSipUri of a DialSip must execute along with the DialSip.");

            // add Sip element
            GetElement(context).Add(
                new XElement("Uri",
                    userName != null ? new XAttribute("username", userName) : null,
                    password != null ? new XAttribute("password", password) : null,
                    uri));
        }

    }

}
