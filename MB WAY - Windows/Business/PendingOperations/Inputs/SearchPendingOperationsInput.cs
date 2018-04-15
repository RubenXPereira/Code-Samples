using Newtonsoft.Json;
using SIBS.MBWAY.Business.Network.AppGeneric.Input;
using SIBS.MBWAY.Business.Utils.Config;

namespace SIBS.MBWAY.Business.Network.PendingOperations.Inputs
{
    public class SearchPendingOperationsInput: GenericInput
    {
        [JsonProperty(JsonTags.kTDAKey)]
        public string tda { get; set; }
    }
}
