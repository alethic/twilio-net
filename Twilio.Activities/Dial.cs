using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Activities.Validation;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Xml.Linq;
using Twilio.Activities.Design;

namespace Twilio.Activities
{

    [Designer(typeof(DialDesigner))]
    public sealed class Dial : TwilioActivity<DialCallStatus>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Dial()
        {
            Activities = new Collection<Activity>();
            Constraints.Add(MustContainAtLeastOneNoun());
        }

        /// <summary>
        /// Validates whether the Dial activity contains at least one DialNoun activity.
        /// </summary>
        /// <returns></returns>
        Constraint<Dial> MustContainAtLeastOneNoun()
        {
            var activityBeingValidated = new DelegateInArgument<Dial>();
            var validationContext = new DelegateInArgument<ValidationContext>();
            var inner = new DelegateInArgument<Activity>();
            var nounIsInner = new Variable<bool>();

            return new Constraint<Dial>()
            {
                Body = new ActivityAction<Dial, ValidationContext>()
                {
                    Argument1 = activityBeingValidated,
                    Argument2 = validationContext,

                    Handler = new Sequence()
                    {
                        Variables = 
                        {
                            nounIsInner,
                        },
                        Activities =
                        {
                            new ForEach<Activity>()
                            {
                                Values = new GetChildSubtree()
                                { 
                                    ValidationContext = validationContext,
                                },
                                Body = new ActivityAction<Activity>()
                                {
                                    Argument = inner,
                                    Handler = new If(env => typeof(DialNoun).IsAssignableFrom(inner.Get(env).GetType()))
                                    {
                                        Then = new Assign<bool>()
                                        { 
                                            To = nounIsInner,
                                            Value = true,
                                        },
                                    },
                                },
                            },
                            new AssertValidation()
                            {
                                Assertion = nounIsInner,
                                Message = "Dial must contain at least one DialNoun",
                                IsWarning = false,
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Timeout before dial is canceled.
        /// </summary>
        public InArgument<TimeSpan?> Timeout { get; set; }

        /// <summary>
        /// Whether if the calling party pressess '*', the they are disconnected and control is returned.
        /// </summary>
        public InArgument<bool?> HangupOnStar { get; set; }

        /// <summary>
        /// Maximum amount of time that can pass before the caller is disconnected.
        /// </summary>
        public InArgument<TimeSpan?> TimeLimit { get; set; }

        /// <summary>
        /// Caller ID to dial as.
        /// </summary>
        public InArgument<string> CallerId { get; set; }

        /// <summary>
        /// Whether the call should be recorded.
        /// </summary>
        public InArgument<bool?> Record { get; set; }

        /// <summary>
        /// The outcome of the dial attempt.
        /// </summary>
        public OutArgument<DialCallStatus> Status { get; set; }

        /// <summary>
        /// The call sid of the new call leg. This parameter is not sent after dialing a conference.
        /// </summary>
        public OutArgument<string> Sid { get; set; }

        /// <summary>
        /// The duration of the dialed call. This parameter is not sent after dialing a conference.
        /// </summary>
        public OutArgument<TimeSpan> Duration { get; set; }

        /// <summary>
        /// The URL of the recorded audio.
        /// </summary>
        public OutArgument<Uri> RecordingUrl { get; set; }

        /// <summary>
        /// Activies executed for the body of Dial.
        /// </summary>
        [Browsable(false)]
        public Collection<Activity> Activities { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var timeout = Timeout.Get(context);
            var hangupOnStar = HangupOnStar.Get(context);
            var timeLimit = TimeLimit.Get(context);
            var callerId = CallerId.Get(context);
            var record = Record.Get(context);

            // dial completion
            var actionUrl = twilio.ResolveBookmarkUrl(context.CreateTwilioBookmark(OnAction));

            // dial element
            var element = new XElement("Dial",
                new XAttribute("action", actionUrl),
                timeout != null ? new XAttribute("timeout", ((TimeSpan)timeout).TotalSeconds) : null,
                hangupOnStar != null ? new XAttribute("hangupOnStar", (bool)hangupOnStar ? "true" : "false") : null,
                timeLimit != null ? new XAttribute("timeLimit", ((TimeSpan)timeLimit).TotalSeconds) : null,
                callerId != null ? new XAttribute("callerId", callerId) : null,
                record != null ? new XAttribute("record", (bool)record ? "true" : "false") : null);

            // write Dial element
            GetElement(context).Add(
                element,
                new XElement("Redirect", actionUrl));

            // execute nouns
            if (Activities.Count > 0)
            {
                // schedule nouns with reference to Dial element
                twilio.SetElement(context, element);
                foreach (var noun in Activities)
                    context.ScheduleActivity(noun);
            }
        }

        /// <summary>
        /// Invoked when digits are available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="o"></param>
        void OnAction(NativeActivityContext context, Bookmark bookmark, object o)
        {
            var r = (NameValueCollection)o;
            var status = r["DialCallStatus"];
            var sid = r["DialCallSid"];
            var duration = r["DialCallDuration"];
            var recordingUrl = r["RecordingUrl"];

            // set output arguments
            Result.Set(context, status != null ? ParseCallStatus(status) : DialCallStatus.Unknown);
            Status.Set(context, status != null ? ParseCallStatus(status) : DialCallStatus.Unknown);
            Sid.Set(context, sid);
            Duration.Set(context, duration != null ? TimeSpan.FromSeconds(int.Parse(duration)) : TimeSpan.Zero);
            RecordingUrl.Set(context, recordingUrl != null ? new Uri(recordingUrl) : null);

            // cancel all children
            context.RemoveAllBookmarks();
            context.CancelChildren();
        }

        /// <summary>
        /// Parses a call status string into the proper type.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        DialCallStatus ParseCallStatus(string s)
        {
            switch (s)
            {
                case "completed":
                    return DialCallStatus.Completed;
                case "busy":
                    return DialCallStatus.Busy;
                case "no-answer":
                    return DialCallStatus.NoAnswer;
                case "failed":
                    return DialCallStatus.Failed;
                case "canceled":
                    return DialCallStatus.Canceled;
                default:
                    throw new FormatException();
            }
        }

    }

}
