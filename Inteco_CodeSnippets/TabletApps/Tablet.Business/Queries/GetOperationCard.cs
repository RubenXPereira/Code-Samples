using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using Imas.Persistence;
using Imas.Common.Models;
using Imas.Domain.Entities;
using static Imas.Common.Models.OperationCardModel;

namespace Imas.Business.Queries
{
    public class GetOperationCard : AbstractQuery
    {
        public GetOperationCard(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public OperationCardModel Execute(string heatNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(heatNumber))
                    return null;

                using (var dbSession = dbFacade.Transaction())
                {
                    var order = dbSession.Set<ProductionOrder>()
                        .Include(x => x.Grade)
                        .Include(x => x.MotherHeat)
                        .Include(x => x.Plant)
                        .Include(x => x.ElectrodeFormat)
                        .Where(x => x.HeatNumber == heatNumber)
                        .SingleOrDefault();

                    if (order == null)
                        return null;

                    var electrode = dbSession.Set<Electrode>()
                        .Where(x => x.Name == heatNumber)
                        .SingleOrDefault();

                    if (electrode == null)
                        return null;

                    var operationCard = dbSession.Set<OperationCard>()
                        .Where(x => x.HeatNumber == heatNumber)
                        .SingleOrDefault();

                    var blockSurfaces = dbSession.Set<CrossSection>().Where(c => c.Type == "BlockSurface").ToList();
                    var Substances = new SubstanceWrapper() { SubstanceList = new List<OperationCardModel.Substance>() };
                    var ISTs = new List<IST>();
                    var Grindings = new List<Grinding>();
                    var Blocks = new List<Block>();

                    if (operationCard != null)
                    {
                        // Signatures
                        var loggedData = dbSession.Set<OperationCardLogs>()
                            .Include(x => x.Operator)
                            .Where(x => x.OperationCardID == operationCard.ID);

                        foreach (var item in loggedData)
                        {
                            switch (item.Type)
                            {
                                case "Substances":
                                    Substances.LogPKID = item.ID;
                                    Substances.Date = item.Date;
                                    Substances.Operator = item.Operator;
                                    break;

                                case "IST":
                                    var ist = new IST()
                                    {
                                        LogPKID = item.ID,
                                        Ist = item.Content,
                                        Date = item.Date,
                                        Operator = item.Operator
                                    };
                                    ISTs.Add(ist);
                                    break;

                                case "Grinding":
                                    var grinding = new Grinding()
                                    {
                                        LogPKID = item.ID,
                                        GrindingTime = int.Parse(item.Content),
                                        Error = item.Error,
                                        Date = item.Date,
                                        Operator = item.Operator
                                    };
                                    Grindings.Add(grinding);
                                    break;

                                case "Block":
                                    var block = new Block()
                                    {
                                        LogPKID = item.ID,
                                        Weight = item.Content,
                                        Date = item.Date,
                                        Operator = item.Operator
                                    };
                                    Blocks.Add(block);
                                    break;

                                default:
                                    throw Oops.Rewrap(new EntryPointNotFoundException($"The signed row of item Type {item.Type} does not have processing implemented."));
                            }
                        }
                    }

                    return new OperationCardModel()
                    {
                        HeatNumber = heatNumber,
                        Customer = order.Customer,
                        Quantity1 = "Missing from ERP", // Missing from ERP
                        BGHNumber = order.Grade.Name,
                        ScrapGroup = order.ScrapGroup,
                        StorageMold = false, // Missing from ERP
                        ElectrodeFormat = order.ElectrodeFormat.Name,
                        ElectrodeLength = electrode.Length,
                        Surface = "Missing from ERP", // Missing from ERP
                        Quantity2 = 0, // Missing from ERP
                        BilletNumber = electrode.BilletNumber,
                        SandBlasting = order.Grade.SandBlasting,
                        SlagNumber = operationCard?.SlagNumber,
                        AGGNumber = order.Plant.AGGNumber,

                        BlockSurfaces = blockSurfaces,
                        BlockSurface = operationCard?.BlockSurfaceID != null ? blockSurfaces.FirstOrDefault(c => c.ID == operationCard.BlockSurfaceID) : null,
                        
                        ErrorCode = operationCard?.ErrorCode,
                        Photo = operationCard?.Photo,
                        Glow = operationCard?.Glow,
                        UserID = operationCard != null ? operationCard.UserID : -1,

                        Substances = Substances,
                        ISTs = ISTs,
                        Grindings = Grindings,
                        Blocks = Blocks,
                    };
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }
    }
}
