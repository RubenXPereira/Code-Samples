using System;
using System.Linq;
using System.Data.Entity;
using Imas.Persistence;
using Imas.Domain.Entities;

namespace Imas.Business.Queries
{
    public class GetChargeData : AbstractQuery
    {
        public GetChargeData(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public Electrode Execute(int productionOrderId)
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    var order = dbSession.Set<ProductionOrder>()
                        .Include(x => x.Grade)
                        .Where(x => x.ID == productionOrderId)
                        .Select(x => new { GradeName = x.Grade.Name })
                        .SingleOrDefault();

                    if (order == null)
                        return null;

                    return new Electrode()
                    {
                    };
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
