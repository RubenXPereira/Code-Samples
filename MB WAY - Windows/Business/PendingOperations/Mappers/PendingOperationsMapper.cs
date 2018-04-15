using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIBS.MBWAY.Business.Network.AppGeneric.Output.Mapper;
using SIBS.MBWAY.Business.Network.PendingOperations.Mappers.JsonModel;
using SIBS.MBWAY.Business.Network.PendingOperations.Models;
using SIBS.MBWAY.Business.Utils.Config;

namespace SIBS.MBWAY.Business.Network.PendingOperations.Mappers
{
    public class PendingOperationsMapper : GenericMapper
    {
        public new PendingOperationsListOutput MapFromJson(string jsonResponse)
        {
            genericOutput = new PendingOperationsListOutput();
            base.MapFromJson(jsonResponse);

            PendingOperationsListOutput pendingOperationsList = (PendingOperationsListOutput)genericOutput;

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                try
                {
                    JObject jObject = JObject.Parse(jsonResponse);
                    var root = jObject.ToObject<RootObject>();

                    if (root != null && root.pendingOperationsList != null)
                    {
                        pendingOperationsList.pendingOperationsList = root.pendingOperationsList;
                    }
                }
                catch (Exception)
                {
                }
            }

            return pendingOperationsList;
        }
    }

    namespace JsonModel
    {
        internal class RootObject
        {
            [JsonProperty(JsonTags.kPendingOperationsListKey)]
            public List<Models.PendingOperation> pendingOperationsList { get; set; }
        }
    }
}
