using System;
using System.Linq;
using System.Collections.Generic;
using Imas.Business;
using Imas.Persistence;
using Imas.Domain.Entities;
using Shop.Common.ViewModels;

namespace Shop.Business.Queries
{
    public class GetMachineEventByPlant : AbstractQuery
    {
        public GetMachineEventByPlant(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public virtual IList<MachineEventViewModel> Execute(int? ingotId, DateTime fromDate)
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    if (!ingotId.HasValue)
                    { return new List<MachineEventViewModel>(); }

                    return dbSession.Set<MachineEvent>()
                        .Where(x =>
                            x.Timestamp >= fromDate &&
                            x.IngotID.HasValue &&
                            x.IngotID.Value == ingotId.Value
                        )
                        .Select(x => new MachineEventViewModel
                        {
                            Name = x.ValueString,
                            Timestamp = x.Timestamp,
                        })
                        .OrderByDescending(x => x.Timestamp)
                        .ToList();
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
