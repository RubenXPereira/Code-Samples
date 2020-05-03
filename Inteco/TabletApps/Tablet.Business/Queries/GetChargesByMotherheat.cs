using System;
using System.Linq;
using System.Data.Entity;
using Imas.Persistence;
using Imas.Domain.Entities;
using System.Collections.Generic;

namespace Imas.Business.Queries
{
    public class GetChargesByMotherheat : AbstractQuery
    {
        public GetChargesByMotherheat(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public IEnumerable<ProductionOrder> Execute(int motherheatID)
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    return dbSession.Set<ProductionOrder>()
                        .Include(x=>x.Grade)
                        .Where(e => e.MotherHeatID == motherheatID && e.ProductionOrderStatus.Name == ProductionOrderStatus.Unplanned)
                        .ToList();
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
