using System.Activities;

namespace Twilio.Activities.Design
{

    public partial class EnqueueDesigner
    {

        public EnqueueDesigner()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            if (ModelItem.Properties["Wait"].Value == null)
                ModelItem.Properties["Wait"].SetValue(new ActivityAction<EnqueueStatus>()
                {
                    Argument = new DelegateInArgument<EnqueueStatus>()
                    {
                        Name = "Status",
                    },
                });

            base.OnModelItemChanged(newItem);
        }

    }

}
