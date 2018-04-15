using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

using SIBS.MBWAY.Windows.Common;
using SIBS.MBWAY.Windows.Utils;
using SIBS.MBWAY.Windows.ViewModels;
using GalaSoft.MvvmLight.Messaging;

namespace SIBS.MBWAY.Windows.Views.Card
{
    public sealed partial class ListCardsPage : BasePage
    {
        public ListCardsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        void AppCustom_onRefreshMasterPageClicked(object sender, EventArgs e)
        {
            AppCustom.mListCardsViewModelInstance = (ListCardsViewModel)DataContext;
            ((ListCardsViewModel)DataContext).LoadDataMainPageAsync(false, false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AppCustom.FireOnShowGamificationButtonMasterPage(true);

            AppCustom.onRefreshMasterPageClicked -= AppCustom_onRefreshMasterPageClicked;
            AppCustom.onRefreshMasterPageClicked += AppCustom_onRefreshMasterPageClicked;

            AppCustom.onAppInitialDataLoadedCards -= AppCustom_onAppInitialDataLoadedCards;
            AppCustom.onAppInitialDataLoadedCards += AppCustom_onAppInitialDataLoadedCards;

            AppCustom.onAppInitialDataLoadedCardsError -= AppCustom_onAppInitialDataLoadedCardsError;
            AppCustom.onAppInitialDataLoadedCardsError += AppCustom_onAppInitialDataLoadedCardsError;

            AppCustom.onAppInitialDataLoadedPendingOperations -= AppCustom_onAppInitialDataLoadedPendingOperations;
            AppCustom.onAppInitialDataLoadedPendingOperations += AppCustom_onAppInitialDataLoadedPendingOperations;

            AppCustom.onLockCodeSuccessForceLoadMainPage -= AppCustom_onLockCodeSuccessForceLoadMainPage;
            AppCustom.onLockCodeSuccessForceLoadMainPage += AppCustom_onLockCodeSuccessForceLoadMainPage;

            Messenger.Default.Send(this);

            if (AppCustom.CardListNeedsRefresh || e.NavigationMode != NavigationMode.Back || (e.NavigationMode == NavigationMode.Back && (AppCustom.SelectedCardWasEdited)))
            {
                AppCustom.CardListNeedsRefresh = false;
                AppCustom.SelectedCardWasEdited = false;
                AppCustom.mListCardsViewModelInstance = (ListCardsViewModel)DataContext;
                ((ListCardsViewModel)DataContext).LoadDataMainPageAsync(true, true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AppCustom.FireOnShowGamificationButtonMasterPage(false);
            AppCustom.onRefreshMasterPageClicked -= AppCustom_onRefreshMasterPageClicked;
            base.OnNavigatedFrom(e);
        }

        void AppCustom_onLockCodeSuccessForceLoadMainPage(object sender, EventArgs e)
        {
            if (!((ListCardsViewModel)DataContext).LoadedServiceParameters && !((ListCardsViewModel)DataContext).LoadedCards && !((ListCardsViewModel)DataContext).LoadedPendingOperations)
                ((ListCardsViewModel)DataContext).LoadDataMainPageAsync(true, false);
        }

        void AppCustom_onAppInitialDataLoadedPendingOperations(object sender, EventArgs e)
        {
            ((ListCardsViewModel)DataContext).LoadedPendingOperations = true;
        }

        void AppCustom_onAppInitialDataLoadedCards(object sender, EventArgs e)
        {
            ((ListCardsViewModel)DataContext).LoadedCards = true;
        }

        void AppCustom_onAppInitialDataLoadedCardsError(object sender, EventArgs e)
        {
            ((ListCardsViewModel)DataContext).LoadedCards = true;
            ((ListCardsViewModel)DataContext).LoadedPendingOperations = true;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
           HelperWindows.LoseFocus(this);
        }

        #region Set Setting Size based on actual screen

        protected override void HandleWindowSizeInfoChanges()
        {
        }

        #endregion
    }
}
