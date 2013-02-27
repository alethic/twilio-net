using System.Activities;
using System.Activities.Statements;
using System.Activities.Validation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Base activity class from which all Twilio activities extend.
    /// </summary>
    public abstract class TwilioActivity : NativeActivity
    {

        /// <summary>
        /// Validates whether the Twilio activity is contained within a CallScope activity.
        /// </summary>
        /// <returns></returns>
        internal static Constraint<Activity> MustBeInsideCallScopeConstraint()
        {
            var activityBeingValidated = new DelegateInArgument<Activity>();
            var validationContext = new DelegateInArgument<ValidationContext>();
            var parent = new DelegateInArgument<Activity>();
            var parentIsCallScope = new Variable<bool>();

            return new Constraint<Activity>()
            {
                Body = new ActivityAction<Activity, ValidationContext>()
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
                                            Value = true,
                                        },
                                    },
                                },
                            },
                            new AssertValidation()
                            {
                                Assertion = parentIsCallScope,
                                Message = "Twilio activities must be nested inside of a CallScope",
                                IsWarning = false,
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal TwilioActivity()
        {
            Variables = new Collection<Variable>();
            Constraints.Add(MustBeInsideCallScopeConstraint());
        }

        [Browsable(false)]
        public Collection<Variable> Variables { get; set; }

        /// <summary>
        /// Gets the current element based on the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected XElement GetElement(NativeActivityContext context)
        {
            return context.GetExtension<ITwilioContext>()
                .GetElement(context);
        }

        /// <summary>
        /// Sets the current element for the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="element"></param>
        protected void SetElement(NativeActivityContext context, XElement element)
        {
            context.GetExtension<ITwilioContext>()
                .SetElement(context, element);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension<ITwilioContext>();
        }

    }

    /// <summary>
    /// Base activity class from which all Twilio activities extend.
    /// </summary>
    public abstract class TwilioActivity<TResult> : NativeActivity<TResult>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal TwilioActivity()
        {
            Variables = new Collection<Variable>();
            Constraints.Add(TwilioActivity.MustBeInsideCallScopeConstraint());
        }

        [Browsable(false)]
        public Collection<Variable> Variables { get; set; }

        /// <summary>
        /// Gets the current element based on the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected XElement GetElement(NativeActivityContext context)
        {
            return context.GetExtension<ITwilioContext>()
                .GetElement(context);
        }

        /// <summary>
        /// Sets the current element for the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="element"></param>
        protected void SetElement(NativeActivityContext context, XElement element)
        {
            context.GetExtension<ITwilioContext>()
                .SetElement(context, element);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension<ITwilioContext>();
        }

    }

}
