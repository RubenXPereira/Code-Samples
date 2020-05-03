using Imas.Domain.Entities;
using Imas.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace Imas.Business.Queries
{
    public class GetAllMotherheats : AbstractQuery
    {
        public GetAllMotherheats(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public IEnumerable<MotherHeat> Execute()
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    return dbSession.Set<ProductionOrder>()
                            .Include(m => m.MotherHeat)
                            .Include(m => m.ProductionOrderStatus)
                            .Where(p => p.ProductionOrderStatus.Name == ProductionOrderStatus.Unplanned)
                            .GroupBy(p => p.MotherHeat.Name)
                            .Select(p => p.FirstOrDefault())
                            .Select(p => p.MotherHeat)
                            .ToList();
                }
            }
            catch (Exception e)
            {
                throw Oops.Rewrap(e);
            }          
        }
    }
}
