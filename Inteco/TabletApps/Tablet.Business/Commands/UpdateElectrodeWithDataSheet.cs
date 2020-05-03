using Imas.Domain.Entities;
using Imas.Persistence;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Imas.Business.Commands
{
    public class UpdateElectrodeWithDataSheet : AbstractCommand
    {
        public UpdateElectrodeWithDataSheet(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public async Task ExecuteAsync(Electrode electrodeModel)
        {
            try
            {
                if (electrodeModel == null)
                    throw new ArgumentNullException(nameof(electrodeModel));

                using (var dbSession = dbFacade.Transaction())
                {
                    var electrode = await dbSession.Set<Electrode>().FirstOrDefaultAsync(e => e.ProductionOrderID == electrodeModel.ProductionOrderID);

                    if (electrode == null)
                    {
                        throw new Exception($"Electrode with id = \"{electrodeModel.ID}\" does not exist.");
                    }

                    electrode.AmountAfterSaw = electrodeModel.AmountAfterSaw;
                    electrode.BilletNumber = electrodeModel.BilletNumber;
                    electrode.Diameter = electrodeModel.Diameter;
                    electrode.Length = electrodeModel.Length;
                    electrode.Photo = electrodeModel.Photo;
                    electrode.SawCut = electrodeModel.SawCut;
                    electrode.StubLength = electrodeModel.StubLength;
                    electrode.StubNumber = electrodeModel.StubNumber;
                    electrode.StubWeight = electrodeModel.StubWeight;
                    electrode.UserID = electrodeModel.UserID;

                    var order = await dbSession.Set<ProductionOrder>()
                        .SingleOrDefaultAsync(p => p.ID == electrodeModel.ProductionOrderID );

                    if (order == null)
                    {
                        throw new Exception($" There is no production order associated with Electrode of given id = \"{electrodeModel.ID}\" ");
                    }

                    var orderStatus = await dbSession.Set<ProductionOrderStatus>()
                        .Where(x => x.Name == ProductionOrderStatus.Planned)
                        .Select(x => new { x.ID })
                        .SingleAsync();

                    order.ProductionOrderStatusID = orderStatus.ID;

                    await dbSession.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            { throw Oops.Rewrap(ex); }
        }
    }
}
