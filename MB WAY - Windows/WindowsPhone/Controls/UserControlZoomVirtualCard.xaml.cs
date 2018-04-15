using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SIBS.MBWAY.Windows.DataModels;
using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml.Media.Animation;

namespace SIBS.MBWAY.Windows.Controls.Cards
{
    public sealed partial class UserControlZoomVirtualCard : UserControl
    {
        public bool IsHiddingData
        {
            get { return (bool)GetValue(IsHiddingDataProperty); }
            set
            {
                SetValue(IsHiddingDataProperty, value);
            }
        }

        public static readonly DependencyProperty IsHiddingDataProperty = DependencyProperty.Register("IsHiddingData",
            typeof(bool), typeof(UserControlZoomVirtualCard), null);

        public bool IsZoomOn
        {
            get { return (bool)GetValue(IsZoomOnProperty); }
            set
            {
                SetValue(IsZoomOnProperty, value);
            }
        }

        public static readonly DependencyProperty IsZoomOnProperty = DependencyProperty.Register("IsZoomOn",
            typeof(bool), typeof(UserControlZoomVirtualCard), null);

        public bool IsAnimating
        {
            get { return (bool)GetValue(IsAnimatingProperty); }
            set
            {
                SetValue(IsAnimatingProperty, value);
            }
        }

        public static readonly DependencyProperty IsAnimatingProperty = DependencyProperty.Register("IsAnimating",
            typeof(bool), typeof(UserControlZoomVirtualCard), null);

        public string VerticalAlignmentUC
        {
            get { return (string) GetValue(VerticalAlignmentUCProperty); }
            set
            {
                SetValue(VerticalAlignmentUCProperty, value);
            }
        }

        public static readonly DependencyProperty VerticalAlignmentUCProperty = DependencyProperty.Register("VerticalAlignmentUC",
            typeof(string), typeof(UserControlZoomVirtualCard), null);

        public string MarginUC
        {
            get { return (string)GetValue(MarginUCProperty); }
            set
            {
                SetValue(MarginUCProperty, value);
            }
        }

        public static readonly DependencyProperty MarginUCProperty = DependencyProperty.Register("MarginUC",
            typeof(string), typeof(UserControlZoomVirtualCard), null);

        public VirtualCard VirtualCardUC
        {
            get { return (VirtualCard)GetValue(VirtualCardProperty); }
            set { SetValue(VirtualCardProperty, value); }
        }

        public static readonly DependencyProperty VirtualCardProperty = DependencyProperty.Register("VirtualCardUC",
            typeof (VirtualCard), typeof (UserControlZoomVirtualCard), null);

        public UserControlZoomVirtualCard()
        {
            InitializeComponent();
            (Content as FrameworkElement).DataContext = this;
        }

        private RelayCommand _closeZoom;

        public RelayCommand CloseZoom
        {
            get
            {
                return _closeZoom ?? (_closeZoom = new RelayCommand(ExecuteCloseZoom));
            }
        }

        private void ExecuteCloseZoom()
        {
            if (IsAnimating)
                return;

            Storyboard mStoryboardReverse = (Storyboard) FindName("animationReverse");
            mStoryboardReverse.Stop();
            mStoryboardReverse.Begin();
            IsAnimating = true;
        }

        private void animation_Completed(object sender, object e)
        {
            IsAnimating = false;
        }

        private void animationReverse_Completed(object sender, object e)
        {
            IsAnimating = false;
            IsZoomOn = false;
        }
    }
}
