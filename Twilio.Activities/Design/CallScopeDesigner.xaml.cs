using System.Activities;
namespace Twilio.Activities.Design
{

    public partial class CallScopeDesigner
    {

        public CallScopeDesigner()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            if (ModelItem.Properties["Body"].Value == null)
                ModelItem.Properties["Body"].SetValue(new ActivityAction<CallContext>()
                {
                    Argument = new DelegateInArgument<CallContext>()
                    {
                        Name = "Call",
                    },
                });

            base.OnModelItemChanged(newItem);
        }

    }

}
