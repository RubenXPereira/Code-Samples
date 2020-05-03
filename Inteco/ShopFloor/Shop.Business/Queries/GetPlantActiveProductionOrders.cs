using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using Imas.Business;
using Imas.Persistence;
using Imas.Domain.Entities;
using Shop.Common.ViewModels;

namespace Shop.Business.Queries
{
    public class GetPlantActiveProductionOrders : AbstractQuery
    {
        public GetPlantActiveProductionOrders(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public virtual IList<ProductionOrderViewModel> Execute()
        {
            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    var productionOrders = dbSession.Set<ProductionOrder>()
                           .Include(x => x.Grade)
                           .Include(x => x.Plant)
                           .Include(x => x.MotherHeat)
                           .Include(x => x.ElectrodeFormat)
                           .Include(x => x.ProductionOrderStatus)
                           .Where(x =>
                               x.PlantID.HasValue &&
                               x.Plant.ActiveIngotID != null &&
                               x.ProductionOrderStatus.Name == ProductionOrderStatus.Production
                           ).ToList();

                    foreach (ProductionOrder item in productionOrders)
                    {
                        item.HasAdditionalData = dbSession.Set<OperationCard>().Any(r => r.ProductionOrderID == item.ID);
                    }

                    return productionOrders.Select(x => new ProductionOrderViewModel
                    {
                        Comments = x.Comments,
                        DeliveryDate = x.DeliveryDate,
                        ElectrodeFormat = x.ElectrodeFormat.Name,
                        GlowGroup = x.GlowGroup,
                        GradeNo = x.Grade.Revision,
                        HeatNumber = x.HeatNumber,
                        MaterialOrderNumber = x.Name,
                        MotherHeatNumber = x.MotherHeat.Name,
                        PlantId = x.PlantID,
                        PlantName = x.Plant.Name,
                        PlantIngotId = x.Plant.ActiveIngotID,
                        ProductionOrderStatus = x.ProductionOrderStatus.Name,
                        RequestWeight = x.RequestWeight,
                        HasAdditionalData = x.HasAdditionalData
                    }).ToList();
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
