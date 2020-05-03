using System;
using System.Linq;
using System.Collections.Generic;
using Imas.Business;
using Imas.Persistence;
using Imas.Domain.Entities;
using Shop.Common.ViewModels;

namespace Shop.Business.Queries
{
    public class GetGroupedMachineDataByPlant : AbstractQuery
    {
        public GetGroupedMachineDataByPlant(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public virtual Tuple<List<MachineDataViewModel>, int> Execute(int ingotId, int fromID, int intendedBatchSize, List<int> availableProcessIDs)
        {
            try
            {
                List<MachineDataViewModel> groupedMachineData = new List<MachineDataViewModel>();

                int currentBatchBeforeGroupingCount = 0;
                int maxID = 0;

                do
                {
                    using (var dbSession = dbFacade.Transaction())
                    {
                        var currentBatchBeforeGrouping = dbSession.Set<MachineData>()
                            .Where(x =>
                                x.IngotID == ingotId &&
                                x.ID >= fromID && x.ID < fromID + intendedBatchSize &&
                                availableProcessIDs.Contains(x.ProcessVariableID)
                            )
                            .OrderBy(x => x.ID);

                        currentBatchBeforeGroupingCount = currentBatchBeforeGrouping.Count();
                        if (currentBatchBeforeGroupingCount > 0)
                        {
                            maxID = currentBatchBeforeGrouping.AsEnumerable().ElementAt(currentBatchBeforeGrouping.Count() - 1).ID;

                            var currentBatch = currentBatchBeforeGrouping
                                .GroupBy(x => x.ProcessVariable.Name)
                                .Select(x => new MachineDataViewModel
                                {
                                    ProcessVariableName = x.Key,
                                    EnumerablePoints = x.Select(z => new DataPointViewModel { Value = z.Value, Timestamp = z.Timestamp }),
                                }).ToList();

                            // Updates
                            fromID += intendedBatchSize;
                            groupedMachineData.AddRange(currentBatch);
                        }
                    }
                }
                while (currentBatchBeforeGroupingCount > 0);

                return Tuple.Create(groupedMachineData, maxID);
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
