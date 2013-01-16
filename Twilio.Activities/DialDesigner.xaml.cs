using System.Activities;
using System.Windows;
using System.Windows.Controls;

namespace Twilio.Activities
{

    public partial class DialDesigner
    {

        public DialDesigner()
        {
            InitializeComponent();
        }

        void BodyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            var cb = (ComboBox)sender;
            switch ((string)cb.SelectedItem)
            {
                case "Number":
                    ModelItem.Properties["Body"].SetValue(new ActivityFunc<DialBody>()
                    {
                        Handler = new DialNumber()
                        {

                        },
                    });
                    break;
                case "Sip":
                    ModelItem.Properties["Body"].SetValue(new ActivityFunc<DialBody>()
                    {
                        Handler = new DialSip()
                        {
                            Uris =
                            {
                                new ActivityFunc<DialSipBodyUri>()
                                {
                                    Handler =  new DialSipUri()
                                    {
                                    
                                    },
                                },
                            },
                        },
                    });
                    break;
            }
        }

        void BodyTypeComboBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var cb = (ComboBox)sender;
            var body = ModelItem.Properties["Body"].Value;
            if (body.ItemType == typeof(ActivityFunc<DialBody>))
                if (body.Properties["Handler"].Value.ItemType == typeof(DialNumber))
                    cb.SelectedItem = "Number";
                else if (body.Properties["Handler"].Value.ItemType == typeof(DialSip))
                    cb.SelectedItem = "Sip";
        }

    }

}
