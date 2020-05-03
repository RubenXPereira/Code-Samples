using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Imas.Persistence;
using Imas.Domain.Entities;

namespace Imas.Business.Queries
{
    public class GetUserByPIN : AbstractQuery
    {
        public GetUserByPIN(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public async Task<User> Execute(string pin)
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    return await dbSession.Set<User>().SingleOrDefaultAsync(x => x.Pin == pin);
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
