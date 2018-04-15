using Newtonsoft.Json;
using SIBS.MBWAY.Business.Network.AppGeneric.Output;
using SIBS.MBWAY.Business.Utils.Config;

namespace SIBS.MBWAY.Business.Network.PendingOperations.Models
{
    public class PendingOperationOutput : GenericOutput
    {
        [JsonProperty(JsonTags.kPendingOperationKey)]
        public PendingOperation pendingOperation { get; set; }
    }
}
