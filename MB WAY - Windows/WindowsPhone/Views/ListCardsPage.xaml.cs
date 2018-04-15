using System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using SIBS.MBWAY.Windows.Common;
using SIBS.MBWAY.Windows.ViewModels;

namespace SIBS.MBWAY.Windows.Views.Card
{
    public sealed partial class ListCardsPage : BasePage
    {
        public ListCardsPage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Disabled;
            flipViewCards.ManipulationStarted += flipViewCards_ManipulationStarted;
        }

        void flipViewCards_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
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
    }
}
