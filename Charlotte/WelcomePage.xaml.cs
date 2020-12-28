/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE', which is part of this source code package.
 */
using ModernWpf.Controls;
using System.Windows.Media;

namespace Charlotte
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage
    {
        public WelcomePage()
        {
            InitializeComponent();

            Brush brush = new SolidColorBrush() { Opacity = 0 };

            this.Background = brush;
            //this.SetValue(VisualState, "DialogShowingWithoutSmokeLayer");

            // TODO: Remove Smoke layer
            // NO SOLUTION FOR NOW
        }

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void OnCloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            deferral.Complete();
        }


        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {

        }


        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    var dialogShowingStates = GetTemplateChild("DialogShowingStates") as VisualStateGroup;
        //    var backgroundElement = GetTemplateChild("BackgroundElement") as FrameworkElement;

        //    FrameworkElement layoutRoot = Parent.GetTemplateChild("LayoutRoot") as FrameworkElement;
        //    //layoutRoot.Background = null;

        //    //dialogShowingStates.GetValue(DialogShowing);

        //    dialogShowingStates.CurrentStateChanged += (s, e) =>
        //    {
        //        //Debug.WriteLine($"OldState: {e.OldState?.Name}, NewState: {e.NewState.Name}");
        //        if (e.NewState.Name == "DialogShowing")
        //        {
        //            VisualStateManager.GoToState(this, "DialogShowingWithoutSmokeLayerState", true);
        //            //dialogShowingStates.SetValue(e, GetTemplateChild("DialogShowingWithoutSmokeLayer") as VisualState);
        //        }
        //    };
        //}


    }
}

