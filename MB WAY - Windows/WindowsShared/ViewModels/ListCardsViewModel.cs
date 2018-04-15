using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using GalaSoft.MvvmLight.Command;
using SIBS.MBWAY.Business.Network.AppGeneric.Output;
using SIBS.MBWAY.Business.Network.Cards;
using SIBS.MBWAY.Business.Network.Utils.Storage;
using SIBS.MBWAY.Windows.Common;
using SIBS.MBWAY.Windows.Mapper;
using SIBS.MBWAY.Windows.Utils;
using SIBS.MBWAY.Windows.Utils.LocalStore.Cards;
using SIBS.MBWAY.Windows.Utils.MbwayPopup;
using SIBS.MBWAY.Windows.Utils.StatusCodeHandlers;
using SIBS.MBWAY.Windows.Utils.StatusCodeHandlers.Handlers;
using SIBS.MBWAY.Windows.Views.Movements;
using SIBS.MBWAY.Windows.Views.Card;
using SIBS.MBWAY.Windows.Views.MasterPage;
using SIBS.MBWAY.Windows.Views.Transfer;
using SIBS.MBWAY.Windows.Utils.StatusCodeHandlers.Handlers.Cards;
using SIBS.MBWAY.Windows.Views;
using SIBS.MBWAY.Windows.Views.Card.MBNETGenerate;
using SIBS.MBWAY.Windows.Views.Card.MBNETCards;
using SIBS.MBWAY.Windows.Views.Payment;
using Windows.System;
using Windows.UI.Xaml;
using SIBS.MBWAY.Business.Security;
using SIBS.MBWAY.Business;

namespace SIBS.MBWAY.Windows.ViewModels
{
    public class ListCardsViewModel : MainViewModel
    {
        #region 'Properties'

        private ObservableCollection<DataModels.Card> _cards;

        public ObservableCollection<DataModels.Card> Cards
        {
            get { return _cards; }
            set
            {
                if (value != _cards)
                {
                    _cards = value;
                    RaisePropertyChanged("Cards");
                }
            }
        }

        private DataModels.Card _selectedCard;

        public DataModels.Card SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if (value != _selectedCard)
                {
                    _selectedCard = value;
                    RaisePropertyChanged("SelectedCard");
                }
            }
        }

        private int selectedIndexCard;

        public int SelectedIndexCard
        {
            get { return selectedIndexCard; }
            set
            {
                if (value != selectedIndexCard)
                {
                    selectedIndexCard = value;
                    RaisePropertyChanged("SelectedIndexCard");

                    SelectedCard = Cards[selectedIndexCard];
                }
            }
        }

        private RelayCommand _goToPaymentCommand;

        public RelayCommand GoToPaymentCommand
        {
            get
            {
                return _goToPaymentCommand
                    ?? (_goToPaymentCommand = new RelayCommand(ExecuteGoToPayment));
            }
        }

        private RelayCommand _goToTransferCommand;

        public RelayCommand GoToTransferCommand
        {
            get
            {
                return _goToTransferCommand
                    ?? (_goToTransferCommand = new RelayCommand(ExecuteGoToTransfers));
            }
        }

        private RelayCommand _goToGenerateCardCommand;

        public RelayCommand GoToGenerateCardCommand
        {
            get
            {
                return _goToGenerateCardCommand
                    ?? (_goToGenerateCardCommand = new RelayCommand(ExecuteGoToGenerateCard));
            }
        }

        private RelayCommand _goToMovementsByCardCommand;

        public RelayCommand GoToMovementsByCardCommand
        {
            get
            {
                return _goToMovementsByCardCommand
                    ?? (_goToMovementsByCardCommand = new RelayCommand(ExecuteGoToMovementsByCard));
            }
        }

        private RelayCommand _goToMBNETCardsCommand;

        public RelayCommand GoToMBNETCardsCommand
        {
            get
            {
                return _goToMBNETCardsCommand
                    ?? (_goToMBNETCardsCommand = new RelayCommand(ExecuteGoToMBNETCards));
            }
        }

        private RelayCommand _goToMBWAYWithdrawalCommand;

        public RelayCommand GoToMBWAYWithdrawalCommand
        {
            get
            {
                return _goToMBWAYWithdrawalCommand
                    ?? (_goToMBWAYWithdrawalCommand = new RelayCommand(ExecuteGoToMBWAYWithdrawal));
            }
        }

        private RelayCommand _goToEditCardCommand;

        public RelayCommand GoToEditCardCommand
        {
            get
            {
                return _goToEditCardCommand
                    ?? (_goToEditCardCommand = new RelayCommand(ExecuteGoToEditCard));
            }
        }

        private RelayCommand _goToRemoveCardCommand;

        public RelayCommand GoToRemoveCardCommand
        {
            get
            {
                return _goToRemoveCardCommand
                    ?? (_goToRemoveCardCommand = new RelayCommand(ExecuteRemoveCard));
            }
        }

        private RelayCommand _exitCommand;

        public RelayCommand ExitCommand
        {
            get
            {
                return _exitCommand
                    ?? (_exitCommand = new RelayCommand(ExecuteExitApp));
            }
        }

        private CardsManager cardsManager = new CardsManager();

        #endregion
        
        public void LoadCardsAsync(bool hideControls)
        {
            if (hideControls) IsToHideControls = true;

            IsLoading = true;

            try
            {
                cardsManager.SynchronizeBankCardData(SynchronizeBankCardDataCallback);
            }
            catch (Exception)
            {
            }
        }

        public async void SynchronizeBankCardDataCallback(Business.Network.Cards.Models.ListCardOutput responseCards, Exception exception)
        {
            try
            {
                if (exception == null && responseCards != null)
                {
                    if (!string.IsNullOrEmpty(responseCards.status))
                    {
                        var handler = new StatusCodeSyncCardDataHandler(false);
                        await handler.HandleStatusCodeResult(responseCards.status);

                        if (handler.StatusCode == StatusCodeHandler.EnumStatusCodes.SUCCESS)
                        {
                            responseCards.cardsList = await LocalStoreCardsHelper.ManageLocalCardsStoreBasedOnSyncCardData(responseCards.cardsList);

                            if (responseCards.cardsList != null && responseCards.cardsList.Count > 0)
                            {
                                fillObservableCardArray(responseCards.cardsList);
                                IsLoading = false;
                                AppCustom.mListCardsViewModelInstance = null;
                                return;
                            }
                        }

                        responseCards.cardsList = await LocalStoreCardsHelper.GetListLocalCardsStore();

                        if (responseCards.cardsList != null && responseCards.cardsList.Count > 0)
                        {
                            fillObservableCardArray(responseCards.cardsList);
                            IsLoading = false;
                            NavigateServiceManager.Instance.HandleCompleteNavigation(handler);
                            AppCustom.mListCardsViewModelInstance = null;
                            return;
                        }

                        IsToHideControls = true;
                        IsLoading = false;

                        // If we reach this case, we want to navigate do the MainPage irrespectively of the Error Type
                        handler.StatusCodeNavigationPage.PageType = HelperWindows.GetPageToNavigateBasedOnTag(HelperWindows.EnumNavigationPages.MainPage.ToString());
                        NavigateServiceManager.Instance.HandleCompleteNavigation(handler);
                    }
                }
                else if (!Helper.IsInternetConnected())
                {
                    responseCards.cardsList = await LocalStoreCardsHelper.GetListLocalCardsStore();
                    IsLoading = false;
                    if (responseCards.cardsList != null)
                    {
                        fillObservableCardArray(responseCards.cardsList);
                    }
                    else
                    {
                        NavigateServiceManager.Instance.Navigate(typeof(MainPage), typeof(MasterPage));
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                AppCustom.mListCardsViewModelInstance = null;
                IsLoading = false;
            }
        }

        // Fills the array of cards with the retrieved data
        public void fillObservableCardArray(System.Collections.Generic.List<Business.Network.Cards.Models.Card> cardsList)
        {
            CardMapper cardMapper = new CardMapper();

            Cards = new ObservableCollection<DataModels.Card>();

            foreach (Business.Network.Cards.Models.Card item in cardsList)
            {
                DataModels.Card card = cardMapper.MapFrom(item);
                // For UI Update
                if (!AppServiceParameters.IsWithdrawalMBWayFunctionalityActive && card.CardInhibitionParameters != null)
                {
                    card.CardInhibitionParameters.IsAllowedToWithdrawalMBWAY = false;
                }
                Cards.Add(card);
            }

            if (SelectedIndexCard >= Cards.Count)
                SelectedIndexCard = 0;
            
            SelectedCard = Cards[SelectedIndexCard];

            checkAndSetAvailableSelectedCard();
        }

        private void checkAndSetAvailableSelectedCard()
        {
            if (StorageUtils.IsStoredFlagActive(StorageUtils.kNotificationApproveTransfer))
            {
                // Has to do 'EnumPushNotificationType.APPROVED_TRANSFER'
                StorageUtils.storeFlagState(StorageUtils.kNotificationApproveTransfer, false);
            }

            if (!SelectedCard.CardP2PParameters.IsDefaultForTransfers)
            {
                DataModels.Card mDefaultCard = getFirstDefaultCard();

                if (mDefaultCard == null)
                {
                    // Default Logic remains the same
                    setSelectedCardToFirstAvailable();
                }
                else
                {
                    SelectedCard = mDefaultCard;
                    SelectedIndexCard = Cards.IndexOf(SelectedCard);
                }
            }
        }

        private DataModels.Card getFirstDefaultCard()
        {
            string lastUsedCardIDC = DataModels.Card.getLastUsedIDC();

            DataModels.Card cardToReturn = null;

            foreach (DataModels.Card card in Cards)
            {
                if (card.CardP2PParameters.IsDefaultForTransfers)
                {
                    cardToReturn = card;
                    break;
                }

                if (card.CardP2PParameters.IsDefaultForPurchases)
                {
                    cardToReturn = card;
                    continue;
                }

                if (cardToReturn == null && card.Idc == lastUsedCardIDC)
                {
                    cardToReturn = card;
                }
            }

            return cardToReturn;
        }

        private void setSelectedCardToFirstAvailable()
        {
            foreach (DataModels.Card card in Cards)
            {
                if (card.CardP2PParameters.AllowedToReceiveTransfers)
                {
                    SelectedCard = card;
                    SelectedIndexCard = Cards.IndexOf(SelectedCard);
                    return;
                }
            }
        }

        public void GoToDetailCard()
        {
            try
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        () =>
                        {
                            NavigateServiceManager.Instance.Navigate(typeof(CardDetailPage), SelectedCard, typeof(MasterPage));
                        });
            }
            catch (Exception)
            {
            }
        }

        public async void CheckGoToTransferPage(int cardIndex)
        {
            try
            {
                if (SelectedCard != null)
                {
                    if (Helper.IsCardOnlyAllowedToMBNet(SelectedCard))
                    {
                        await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, ResourceLoader.GetForCurrentView().GetString("ListCards_Popup_MBNET_Only"),
                                MbwayPopupManager.MbwayPopupDefaultButtons.Ok);

                        return;
                    }

                    if (SelectedCard.CardP2PParameters != null && !SelectedCard.CardP2PParameters.AllowedToMakeTransfers)
                    {
                        ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

                        await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, resourceLoader.GetString("ListCards_Popup_NotAllowedToMakeTransfers"),
                                MbwayPopupManager.MbwayPopupDefaultButtons.Ok);
                    }
                    else
                    {
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal,
                            () =>
                            {
                                NavigateServiceManager.Instance.Navigate(typeof(TransferPage), SelectedCard, // cardIndex
                                    typeof(MasterPage));
                            });
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public async void GoToPayment()
        {
            if (Helper.IsCardOnlyAllowedToMBNet(SelectedCard))
            {
                await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, ResourceLoader.GetForCurrentView().GetString("ListCards_Popup_MBNET_Only"),
                        MbwayPopupManager.MbwayPopupDefaultButtons.Ok);

                return;
            }

            try
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        () =>
                        {
                            NavigateServiceManager.Instance.Navigate(typeof(PaymentPage), typeof(MasterPage));
                        });
            }
            catch (Exception e)
            {
                LogManager.WriteLine(e.Message);
            }
        }

        public async void GoToGenerateCard()
        {
            try
            {
                if (SelectedCard != null)
                {
                    if (SelectedCard.CardMBNETParameters != null && !SelectedCard.CardMBNETParameters.AllowedToCreateVirtualCards)
                    {
                        ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

                        await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, resourceLoader.GetString("ListCards_Popup_NotAllowedToAccessMBNET"),
                                MbwayPopupManager.MbwayPopupDefaultButtons.Ok);
                    }
                    else
                    {
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal,
                            () =>
                            {
                                NavigateServiceManager.Instance.Navigate(typeof(GenerateCard), SelectedCard, typeof(MasterPage));
                            });
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public async void GoToMovementsCard()
        {
            if (Helper.IsCardOnlyAllowedToMBNet(SelectedCard))
            {
                await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, ResourceLoader.GetForCurrentView().GetString("ListCards_Popup_MBNET_Only"),
                        MbwayPopupManager.MbwayPopupDefaultButtons.Ok);

                return;
            }

            try
            {
                string codAut = await new ApplicationMainPopupManager().ShowPopupInputPinMbWayAsync(false);

                if (!string.IsNullOrEmpty(codAut))
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        () =>
                        {
                            NavigateServiceManager.Instance.Navigate(typeof(ListMovementsByCard), SelectedCard, typeof(MasterPage));
                        });
                }
            }
            catch (Exception)
            {
            }
        }

        public async void GoToMBNETCards()
        {
            try
            {
                if (SelectedCard != null)
                {
                    if (SelectedCard.CardMBNETParameters != null && !SelectedCard.CardMBNETParameters.AllowedToCreateVirtualCards)
                    {
                        ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

                        await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, resourceLoader.GetString("ListCards_Popup_NotAllowedToAccessMBNET"),
                                MbwayPopupManager.MbwayPopupDefaultButtons.Ok);
                    }
                    else
                    {
                        string codAut = await new ApplicationMainPopupManager().ShowPopupInputPinMbWayAsync(false);

                        if (!string.IsNullOrEmpty(codAut))
                        {
                            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal,
                            () =>
                            {
                                NavigateServiceManager.Instance.Navigate(typeof(ListVirtualCardsPage), SelectedCard, typeof(MasterPage));
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public async void GoToMBWAYWithdrawal()
        {
            if (Helper.IsCardOnlyAllowedToMBNet(SelectedCard))
            {
                await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, ResourceLoader.GetForCurrentView().GetString("ListCards_Popup_MBNET_Only"),
                        MbwayPopupManager.MbwayPopupDefaultButtons.Ok);

                return;
            }

            if (SelectedCard != null)
            {
                if ((SelectedCard.CardInhibitionParameters != null && !SelectedCard.CardInhibitionParameters.IsAllowedToWithdrawalMBWAY) || !AppServiceParameters.IsWithdrawalMBWayFunctionalityActive)
                {
                    if (!AppServiceParameters.IsWithdrawalMBWayFunctionalityActive)
                    {
                        ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("ResourcesErrorCodes");
                        await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, resourceLoader.GetString("error_code_186_187_request_withdrawal"),
                            MbwayPopupManager.MbwayPopupDefaultButtons.Ok);
                    }
                    else
                    {
                        ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
                        await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Warning, resourceLoader.GetString("ListCards_Popup_NotAllowedToMakeWithdrawals"),
                            MbwayPopupManager.MbwayPopupDefaultButtons.Ok);
                    }
                }
                else
                {
                    NavigateServiceManager.Instance.Navigate(typeof(WithdrawalPage), SelectedCard, typeof(MasterPage));
                }
            }
        }

        public async void RemoveCard()
        {
            try
            {
                ResourceLoader resourceLoader = new ResourceLoader();

                bool removeResult = await new ApplicationMainPopupManager().ShowPopupAsync(MbwayPopupManager.MbwayPopupType.Delete, string.Format(resourceLoader.GetString("ListCards_RemoveCardMessageFormat"), SelectedCard.Name), MbwayPopupManager.MbwayPopupDefaultButtons.OkCancel);

                if (removeResult)
                {
                    string codAut = await new ApplicationMainPopupManager().ShowPopupInputPinMbWayAsync();

                    if (!string.IsNullOrEmpty(codAut))
                    {
                        IsLoading = true;

                        if (cardsManager != null) cardsManager.DisassociateBankCardData(codAut, SelectedCard.Idc, DisassociateBankCardDataCallback);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void ExitApp()
        {
            try
            {
                Application.Current.Exit();
            }
            catch (Exception)
            {
            }
        }

        private async void DisassociateBankCardDataCallback(GenericOutput genericOutput, Exception exception)
        {
            try
            {
                if (exception == null)
                {
                    if (!string.IsNullOrEmpty(genericOutput.status))
                    {
                        var handler = new StatusCodeDisassociateCardHandler();
                        await handler.HandleStatusCodeResult(genericOutput.status);

                        if (handler.StatusCode == StatusCodeHandler.EnumStatusCodes.SUCCESS)
                        {
                            bool result = await LocalStoreCardsHelper.DeleteCardFromLocalCardsStore(SelectedCard.Idc);

                            if (result)
                            {
                                if (SelectedCard.CardP2PParameters.IsDefaultForPurchases)
                                {
                                    StorageUtils.removeData(StorageUtils.kCardDefaultForPurchases);
                                }

                                Cards.Remove(Cards.FirstOrDefault(c => c.Idc == SelectedCard.Idc));
                            }
                        }
                        IsLoading = false;
                        NavigateServiceManager.Instance.HandleCompleteNavigation(handler);
                    }
                }
                IsLoading = false;
            }
            catch (Exception)
            {
                IsLoading = false;
            }
        }


        private void ExecuteGoToPayment() { GoToPayment(); }

        private void ExecuteGoToTransfers()
        {
            try
            {
                int index = Cards.IndexOf(SelectedCard);
                CheckGoToTransferPage(index);
            }
            catch (Exception)
            {
            }
        }

        private void ExecuteGoToGenerateCard() { GoToGenerateCard(); }

        private void ExecuteGoToMovementsByCard() { GoToMovementsCard(); }

        private void ExecuteGoToMBNETCards() { GoToMBNETCards(); }

        private void ExecuteGoToMBWAYWithdrawal() { GoToMBWAYWithdrawal(); }

        private void ExecuteGoToEditCard() { GoToDetailCard(); }

        private void ExecuteRemoveCard() { RemoveCard(); }

        private void ExecuteExitApp() { ExitApp(); }
    }
}
