using System;
using SIBS.MBWAY.Business.Network.PendingOperations.Providers;
using SIBS.MBWAY.Business.Utils;
using SIBS.MBWAY.Business.Network.PendingOperations.Models;
using SIBS.MBWAY.Business.Network.AppGeneric.Output;

namespace SIBS.MBWAY.Business.Network.PendingOperations
{
    public class PendingOperationsManager
    {
        private RestProvider restProvider;
        
        public void GetPendingOperationsList(Action<PendingOperationsListOutput, Exception> callback, string operationCode = null)
        {
            restProvider = new ProviderGetPendingOperationsList(callback, operationCode);
        }

        public void GetPendingOperation(string codAut, string operationCode, Action<PendingOperationOutput, Exception> callback)
        {
            restProvider = new ProviderGetPendingOperation(codAut, operationCode, callback);
        }

        public void ModifyRegistrationAlias(string CodAut, string Alias, int AliasTypeCode, Action<GenericOutput, Exception> callback)
        {
            restProvider = new ProviderModifyRegistrationAlias(CodAut, Alias, AliasTypeCode, callback);
        }

        public void ConfirmMerchantAlias(string CodAut, int operationCode, Action<ResponseConfirmMerchantAlias, Exception> callback)
        {
            restProvider = new ProviderConfirmMerchantAlias(CodAut, operationCode, callback);
        }

        public void RejectMerchantAlias(string CodAut, int operationCode, Action<ResponseRejectMerchantAlias, Exception> callback)
        {
            restProvider = new ProviderRejectMerchantAlias(CodAut, operationCode, callback);
        }

        public void FinancialOperationConfirmation(string CodAut, string IDC, string operationCode, Action<GenericOutput, Exception> callback)
        {
            restProvider = new ProviderFinancialOperationConfirmation(CodAut, IDC, operationCode, callback);
        }

        public void RejectFinancialOperation(string IDC, string operationCode, Action<GenericOutput, Exception> callback)
        {
            restProvider = new ProviderRejectFinancialOperation(IDC, operationCode, callback);
        }

        public void DismissOperation(Action<GenericOutput, Exception> callback, string operationCode)
        {
            restProvider = new ProviderDismissOperation(callback, operationCode);
        }
    }
}
