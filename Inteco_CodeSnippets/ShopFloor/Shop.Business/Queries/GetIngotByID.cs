using System;
using System.Linq;
using Imas.Business;
using Imas.Persistence;
using Imas.Domain.Entities;

namespace Shop.Business.Queries
{
    public class GetIngotByID : AbstractQuery
    {
        public GetIngotByID(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public virtual Ingot Execute(int ingotId)
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    return dbSession.Set<Ingot>()
                        .FirstOrDefault(x => x.ID == ingotId);
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
