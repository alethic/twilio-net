using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Hosting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Twilio.Activities.Design
{

    public partial class WorkflowItemsPresenterSequenceConnector : UserControl, IComponentConnector
    {

        public static readonly DependencyProperty AllowedItemTypeProperty = DependencyProperty.Register("AllowedItemType",
            typeof(Type), typeof(WorkflowItemsPresenterSequenceConnector), new UIPropertyMetadata(typeof(object)));

        public Type AllowedItemType
        {
            get { return (Type)GetValue(AllowedItemTypeProperty); }
            set { base.SetValue(AllowedItemTypeProperty, value); }
        }

        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register("Context",
            typeof(EditingContext), typeof(WorkflowItemsPresenterSequenceConnector));

        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { base.SetValue(ContextProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public WorkflowItemsPresenterSequenceConnector()
        {
            InitializeComponent();
        }

        void CheckAnimate(DragEventArgs args, string storyboardResourceName)
        {
            if (!args.Handled)
            {
                if (!Context.Items.GetValue<ReadOnlyState>().IsReadOnly &&
                    DragDropHelper.AllowDrop(args.Data, Context, new[] { AllowedItemType }))
                    BeginStoryboard((Storyboard)Resources[storyboardResourceName]);
                else
                {
                    args.Effects = DragDropEffects.None;
                    args.Handled = true;
                }
            }
        }

        protected override void OnDragEnter(DragEventArgs args)
        {
            CheckAnimate(args, "Expand");
            dropTarget.Visibility = Visibility.Visible;
        }

        protected override void OnDragLeave(DragEventArgs args)
        {
            CheckAnimate(args, "Collapse");
            dropTarget.Visibility = Visibility.Collapsed;
        }

        protected override void OnDrop(DragEventArgs args)
        {
            dropTarget.Visibility = Visibility.Collapsed;
            base.OnDrop(args);
        }

    }

}
