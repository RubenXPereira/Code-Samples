using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SIBS.MBWAY.Windows.Common;
using SIBS.MBWAY.Windows.ViewModels;
using SIBS.MBWAY.Windows.Utils;
using Windows.UI.Xaml.Input;
using SIBS.MBWAY.Windows.Utils.Helpers;
using Windows.ApplicationModel.Contacts;
using Windows.System;
using SIBS.MBWAY.Windows.Utils.Transfer;
using SIBS.MBWAY.Business.Network.Utils;
using GalaSoft.MvvmLight.Messaging;

namespace SIBS.MBWAY.Windows.Views.Card
{
    public sealed partial class WithdrawalPage : BasePage
    {
        private ContactPicker cp;

        private string contactNumberBackup;

        public WithdrawalPage()
        {
            InitializeComponent();

            HandleWindowSizeInfoChanges();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && e.NavigationMode != NavigationMode.Back)
            {
                ((WithdrawalViewModel)DataContext).Card = (DataModels.Card)e.Parameter;
            }

            ((WithdrawalViewModel)DataContext).initializeContent();

            ((WithdrawalViewModel)DataContext).buttonOk = ButtonOk;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ((WithdrawalViewModel)DataContext).resetContent();
            base.OnNavigatedFrom(e);
        }

        private void Button_ClickOK(object sender, RoutedEventArgs e)
        {
            if (((WithdrawalViewModel)DataContext).validateContent(false, true))
            {
                ((WithdrawalViewModel)DataContext).RequestWithdrawalMBWAY();
            }
        }

        #region 'Amount OnKeyUp'

        private void amountTb_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            HelperUserControlsUI.ValidateWithdrawalAmountOnKeyUp(withdrawalAmount);

            ((WithdrawalViewModel)DataContext).validateContent(true, false);
        }

        #endregion

        private void WithdrawalToOther_Click(object sender, RoutedEventArgs e)
        {
            ((WithdrawalViewModel)DataContext).toggleShowContactFields();
            ButtonOk.Focus(FocusState.Programmatic);
        }

        private void ContactTb_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            // Deny the use of "." "," "-" 
            if (e.Key != VirtualKey.Back)
            {
                // code 190 = .
                if (e.Key.ToString().Equals("190"))
                {
                    txt.Text = txt.Text.Replace(".", "");
                    txt.SelectionStart = txt.Text.Length;
                }
                else
                // code 188 = ,
                if (e.Key.ToString().Equals("188"))
                {
                    txt.Text = txt.Text.Replace(",", "");
                    txt.SelectionStart = txt.Text.Length;
                }
                else
                // code 189 = -
                if (e.Key.ToString().Equals("189"))
                {
                    txt.Text = txt.Text.Replace("-", "");
                    txt.SelectionStart = txt.Text.Length;
                }

                if (!AppDefConstants.INTERNACIONAL_NUMBER_ON)
                {
                    if (ContactTb.Text.Length < 9)
                    {
                        contactNumberBackup = txt.Text.ToString();
                    }
                    else if (ContactTb.Text.Length == 9)
                    {
                        contactNumberBackup = txt.Text.ToString();
                        ButtonOk.Focus(FocusState.Programmatic);
                    }
                    else
                    {
                        txt.Text = contactNumberBackup;
                        ButtonOk.Focus(FocusState.Programmatic);
                    }
                }
                else
                {
                    if (ContactTb.Text.Length < 11)
                    {
                        contactNumberBackup = txt.Text.ToString();
                    }
                    else if (ContactTb.Text.Length == 11)
                    {
                        contactNumberBackup = txt.Text.ToString();
                    }
                    else
                    {
                        txt.Text = contactNumberBackup;
                    }
                }
            }
        }

        private async void pickContactsBt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cp = new ContactPicker();
                cp.SelectionMode = ContactSelectionMode.Fields;
                cp.DesiredFieldsWithContactFieldType.Add(ContactFieldType.PhoneNumber);
                var contact = await cp.PickContactAsync();

                Country selectedCountry = ((WithdrawalViewModel)DataContext).SelectedCountry;
                bool validationResult;
                if (contact == null)
                {
                    validationResult = Helper.ValidateContactInfo("", "", out ((WithdrawalViewModel)DataContext).contactNumberWellFormatted, ref selectedCountry, ref ((WithdrawalViewModel)DataContext).isToResetCountry, ref ((WithdrawalViewModel)DataContext).validateContactInfoAgain);
                    ((WithdrawalViewModel)DataContext).SelectedCountry = selectedCountry;
                    ((WithdrawalViewModel)DataContext).updateUserInterfaceBasedOnBasedOnValidationResult("", "", validationResult);
                }
                else
                {
                    validationResult = Helper.ValidateContactInfo(contact.DisplayName, contact.Phones.FirstOrDefault().Number, out ((WithdrawalViewModel)DataContext).contactNumberWellFormatted, ref selectedCountry, ref ((WithdrawalViewModel)DataContext).isToResetCountry, ref ((WithdrawalViewModel)DataContext).validateContactInfoAgain);
                    ((WithdrawalViewModel)DataContext).SelectedCountry = selectedCountry;
                    ((WithdrawalViewModel)DataContext).updateUserInterfaceBasedOnBasedOnValidationResult(contact.DisplayName, contact.Phones.FirstOrDefault().Number, validationResult);

                    contactNumberBackup = contact.Phones.FirstOrDefault().Number;
                    ButtonOk.Focus(FocusState.Programmatic);
                }
            }
            catch (Exception) { }

            HelperWindows.LoseFocus(sender);
        }

        private void ListPickerButton_Click(object sender, RoutedEventArgs e)
        {
            AppCustom.IsShowedCountryPicker = true;

            var mbWayPickerInfo = new MbWayPickerInfo()
            {
                ShowCountryPicker = true
            };

            Messenger.Default.Send(mbWayPickerInfo);
        }

        #region Set Setting Size based on actual screen

        protected override void HandleWindowSizeInfoChanges()
        {
            gridContent.Height = Window.Current.Bounds.Height - AppSettings.WindowsHeightOtherContent;
        }

        #endregion
    }
}
