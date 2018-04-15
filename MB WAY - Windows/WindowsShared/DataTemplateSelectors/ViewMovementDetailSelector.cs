using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SIBS.MBWAY.Windows.DataModels;

namespace SIBS.MBWAY.Windows.Styles.DataTemplateSelectors
{
    public class ViewMovementDetailSelector : DataTemplateSelector
    {
        public DataTemplate TransferReceivedDataTemplate { get; set; }
        public DataTemplate TransferSentDataTemplate { get; set; }
        public DataTemplate MerchantDataTemplate { get; set; }
        public DataTemplate VirtualCardMovementDetailsTemplate { get; set; }
        public DataTemplate WithdrawalMovementDetailsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Movement movementItem = item as Movement;

            if (movementItem != null)
            {
                if (movementItem.FinancialOperation == null)
                    return base.SelectTemplateCore(item, container); // In case the list is empty, SelectedMovement is non-null, it has "IsVirtualCardOperation = false" to guarantee VC invisibility

                if (movementItem.FinancialOperation.WithdrawalMBWAY != null)
                    return WithdrawalMovementDetailsTemplate;

                if (movementItem.IsVirtualCardOperation == true)
                    return VirtualCardMovementDetailsTemplate;

                if (movementItem.FinancialOperation.Merchant != null && !string.IsNullOrEmpty(movementItem.FinancialOperation.Merchant.MrchtNm))
                {
                    return MerchantDataTemplate;
                }

                if (movementItem.FinancialOperation.TransferPendingOperation != null &&
                    !string.IsNullOrEmpty(movementItem.FinancialOperation.TransferPendingOperation.ParticipantRoleTypeCode))
                {
                    if ((TransferPendingOperation.EnumParticipantRoleTypeCode)
                            Int32.Parse(movementItem.FinancialOperation.TransferPendingOperation.ParticipantRoleTypeCode) ==
                        TransferPendingOperation.EnumParticipantRoleTypeCode.Sender)
                    {
                        return TransferReceivedDataTemplate;
                    }

                    if ((TransferPendingOperation.EnumParticipantRoleTypeCode)
                            Int32.Parse(movementItem.FinancialOperation.TransferPendingOperation.ParticipantRoleTypeCode) ==
                        TransferPendingOperation.EnumParticipantRoleTypeCode.Receiver)
                    {
                        return TransferSentDataTemplate;
                    }
                }

                // Default: shows what comes from the server
                return VirtualCardMovementDetailsTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
