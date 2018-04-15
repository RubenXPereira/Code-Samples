using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using SIBS.MBWAY.Windows.Common;
using SIBS.MBWAY.Windows.ViewModels;
using SIBS.MBWAY.Windows.Utils;
using SIBS.MBWAY.Windows.DataModels;
using GalaSoft.MvvmLight.Messaging;
using SIBS.MBWAY.Windows.Common.Base;

namespace SIBS.MBWAY.Windows.Views.Card
{
    public sealed partial class OngoingWithdrawalPage : BasePage
    {
        public OngoingWithdrawalPage()
        {
            InitializeComponent();

            HandleWindowSizeInfoChanges();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Messenger.Default.Register<MbWaySaveWithdrawalCounter>(this, SaveWithdrawalCounter);

            WithdrawalMBWAY withdrawalMBWAY;

            if (e.Parameter != null)
            {
                withdrawalMBWAY = (WithdrawalMBWAY)e.Parameter;
                ((OngoingWithdrawalViewModel)DataContext).initializeContent(withdrawalMBWAY);
            }
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void Button_ClickOK(object sender, RoutedEventArgs e)
        {
            ((OngoingWithdrawalViewModel)DataContext).finalizeAction();
        }

        public void SaveWithdrawalCounter(MbWaySaveWithdrawalCounter saveWithdrawalCounter)
        {
            ((OngoingWithdrawalViewModel)DataContext).SaveTimeToLiveCounterFreezeTimestamp();            
        }

        #region Set Setting Size based on actual screen

        protected override void HandleWindowSizeInfoChanges()
        {
            gridContent.Height = Window.Current.Bounds.Height - AppSettings.WindowsHeightOtherContent;
        }

        #endregion
    }
}
