using System;
using System.Linq;
using Imas.Persistence;
using Imas.Domain.Entities;
using Imas.Common.Models;
using System.Data.Entity;

namespace Imas.Business.Queries
{
    public class GetTabletHeaderData : AbstractQuery
    {
        public GetTabletHeaderData(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public HeaderData Execute(string heatNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(heatNumber))
                    return null;

                using (var dbSession = dbFacade.Transaction())
                {
                    return dbSession.Set<ProductionOrder>()
                        .Include(x=> x.MotherHeat)
                        .Include(x => x.Grade)
                        .Include(x => x.ElectrodeFormat)
                        .Where(x => x.HeatNumber == heatNumber)
                        .Select(x => new HeaderData
                        {
                            MotherHeatNumber = x.MotherHeat.Name,
                            MaterialOrderNumber = x.Name,
                            HeatNumber = x.HeatNumber,
                            MaterialNumber = x.Grade.Name,
                            DeliveryDate = x.DeliveryDate,
                            RawElectrodeFormat = x.ElectrodeFormat.Name,
                            GlowGroup = x.GlowGroup,
                            Comments = x.Comments
                        }).SingleOrDefault();
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
