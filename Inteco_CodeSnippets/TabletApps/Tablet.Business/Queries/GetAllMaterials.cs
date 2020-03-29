using System;
using System.Collections.Generic;
using System.Linq;
using Imas.Domain.Entities;
using Imas.Persistence;
using System.Data.Entity;

namespace Imas.Business.Queries
{
    public class GetAllMaterials : AbstractQuery
    {
        public GetAllMaterials(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public virtual IEnumerable<Material> ExecuteWithMaterialTypeAndMaterialElements()
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    return dbSession.Set<Material>()
                        .Include(material => material.MaterialType)
                        .Include(material => material.MaterialElements.Select(me => me.Element))
                        .ToList();
                }
            }

            catch (Exception ex)
            {
                throw Oops.Rewrap(ex);
            }
        }
    }
}
