using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SIBS.MBWAY.Business.Network.PendingOperations.Inputs;
using SIBS.MBWAY.Business.Network.PendingOperations.Mappers;
using SIBS.MBWAY.Business.Network.PendingOperations.Models;
using SIBS.MBWAY.Business.Network.Utils;
using SIBS.MBWAY.Business.Network.Utils.Storage;
using SIBS.MBWAY.Business.Utils;
using SIBS.MBWAY.Business.Security;

namespace SIBS.MBWAY.Business.Network.PendingOperations.Providers
{
    public class ProviderGetPendingOperationsList : RestProvider
    {
        private Action<PendingOperationsListOutput, Exception> callback;
        private string operationCode;

        public ProviderGetPendingOperationsList(Action<PendingOperationsListOutput, Exception> callback, string operationCode)
        {
            this.callback = callback;
            this.operationCode = operationCode;
            handleRequest();
        }

        protected override void handleRequest()
        {
            if (string.IsNullOrEmpty(operationCode))
            {
                input = new SearchPendingOperationsInput()
                {
                    ida = StorageUtils.getIDA(),
                    appVersion = AppSetup.getAppVersion(),
                    tda = TDA.getEvolvedTDA(),
                    messageCode = RequestCodes.REQ024,
                    messageVersion = int.Parse(RequestCodes.REQ024Version)
                };

                // In case of TDA retrieving error, do not make the request
                if (((SearchPendingOperationsInput)input).tda == null)
                {
                    handleResponse("Error getting evolved TDA");
                    return;
                }
            }
            else
            {
                input = new SearchPendingOperationsInputWithOc()
                {
                    ida = StorageUtils.getIDA(),
                    appVersion = AppSetup.getAppVersion(),
                    tda = TDA.getEvolvedTDA(),
                    messageCode = RequestCodes.REQ024,
                    messageVersion = int.Parse(RequestCodes.REQ024Version),
                    operationCode = operationCode
                };

                // In case of TDA retrieving error, do not make the request
                if (((SearchPendingOperationsInputWithOc)input).tda == null)
                {
                    handleResponse("Error getting evolved TDA");
                    return;
                }
            }

            // TODO
            
            postRequest();
        }

        protected override async void handleResponse(string response)
        {
#if DUMMY
            var s = Assembly.Load(new AssemblyName("SIBS.MBWAY.Business")).GetManifestResourceStream(@"SIBS.MBWAY.Business.FakeJson.PendingOperations.C024.json");
            response = new StreamReader(s).ReadToEnd();

            await Task.Delay(TimeSpan.FromMilliseconds(800));
#endif
            PendingOperationsListOutput pendingOperationsList = new PendingOperationsMapper().MapFromJson(response);
            callback.Invoke(pendingOperationsList, null);
        }
    }
}
