using System;
using System.Linq;
using Imas.Business;
using Imas.Persistence;
using Imas.Domain.Entities;

namespace Shop.Business.Queries
{
    public class ValidateUserPIN : AbstractQuery
    {
        public ValidateUserPIN(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public virtual bool Execute(string pin)
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    return (dbSession.Set<User>().Where(x => x.Pin == pin).Count() > 0);
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
