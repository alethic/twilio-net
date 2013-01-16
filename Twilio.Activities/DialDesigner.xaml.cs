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
                    {
                        var dialNounDelegateArg = new DelegateOutArgument<DialNoun>();
                        var dialNounArg = new OutArgument<DialNoun>(dialNounDelegateArg);

                        // create func
                        var dialNounFunc = new ActivityFunc<DialNoun>();
                        var dialNounFuncModelItem = ModelItem.Properties["Noun"].SetValue(dialNounFunc);
                        dialNounFuncModelItem.Properties["Result"].SetValue(dialNounDelegateArg);

                        // create dial activity
                        var dialNumber = new DialNumber();
                        var dialNumberModelItem = dialNounFuncModelItem.Properties["Handler"].SetValue(dialNumber);
                        dialNumberModelItem.Properties["Result"].SetValue(dialNounArg);
                    }
                    break;
                case "Sip":
                    {
                        var dialNounDelegateArg = new DelegateOutArgument<DialNoun>();
                        var dialNounArg = new OutArgument<DialNoun>(dialNounDelegateArg);

                        // create func
                        var dialNounFunc = new ActivityFunc<DialNoun>();
                        var dialNounFuncModelItem = ModelItem.Properties["Noun"].SetValue(dialNounFunc);
                        dialNounFuncModelItem.Properties["Result"].SetValue(dialNounDelegateArg);

                        // create dial activity
                        var dialSip = new DialSip();
                        var dialSipModelItem = dialNounFuncModelItem.Properties["Handler"].SetValue(dialSip);
                        dialSipModelItem.Properties["Result"].SetValue(dialNounArg);

                        var dialSipUriNounDelegateArg = new DelegateOutArgument<DialSipUriNoun>();
                        var dialSipUriNounArg = new OutArgument<DialSipUriNoun>(dialSipUriNounDelegateArg);

                        var dialSipUriFunc = new ActivityFunc<DialSipUriNoun>();
                        var dialSipUriFuncModelItem = dialSipModelItem.Properties["Uris"].Collection.Add(dialSipUriFunc);
                        dialSipUriFuncModelItem.Properties["Result"].SetValue(dialSipUriNounDelegateArg);

                        var dialSipUri = new DialSipUri();
                        var dialSipUriModelItem = dialSipUriFuncModelItem.Properties["Handler"].SetValue(dialSipUri);
                        dialSipUriModelItem.Properties["Result"].SetValue(dialSipUriNounArg);
                    }
                    break;
            }
        }

        void BodyTypeComboBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var cb = (ComboBox)sender;
            var body = ModelItem.Properties["Noun"].Value;
            if (body.ItemType == typeof(ActivityFunc<DialNoun>))
                if (body.Properties["Handler"].Value.ItemType == typeof(DialNumber))
                    cb.SelectedItem = "Number";
                else if (body.Properties["Handler"].Value.ItemType == typeof(DialSip))
                    cb.SelectedItem = "Sip";
        }

    }

}
