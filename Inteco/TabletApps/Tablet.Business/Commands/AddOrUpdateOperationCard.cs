using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Imas.Persistence;
using Imas.Common.Models;
using Imas.Domain.Entities;
using System.Linq;
using System.Collections.Generic;
using Imas.Business.Queries;

namespace Imas.Business.Commands
{
    public class AddOrUpdateOperationCard : AbstractCommand
    {
        public AddOrUpdateOperationCard(IDbFacade dbFacade)
            : base(dbFacade)
        { }

        public async Task<string> ExecuteAsync(OperationCardModel operationCardModel)
        {
            if (operationCardModel == null)
                throw new ArgumentNullException(nameof(operationCardModel));

            try
            {
                using (var dbSession = dbFacade.Transaction())
                {
                    OperationCard operationCard = await dbSession.Set<OperationCard>()
                        .FirstOrDefaultAsync(u => u.HeatNumber == operationCardModel.HeatNumber);

                    if (operationCard == null)
                    {
                        // Check if a Production Order corresponding to that HeatNumber exists
                        if (!await dbSession.Set<ProductionOrder>().AnyAsync(u => u.HeatNumber == operationCardModel.HeatNumber))
                        {
                            return "No ProductionOrder for the inserted HeatNumber";
                        }

                        // (Create)
                        int productionOrderID = dbSession.Set<ProductionOrder>().FirstOrDefault(u => u.HeatNumber == operationCardModel.HeatNumber).ID;
                        int instructionID = dbSession.Set<Instruction>().FirstOrDefault(u => u.ProductionOrderID == productionOrderID).ID;

                        operationCard = new OperationCard
                        {
                            HeatNumber = operationCardModel.HeatNumber,
                            SlagNumber = operationCardModel.SlagNumber,
                            BlockSurfaceID = operationCardModel.BlockSurface?.ID,
                            ErrorCode = operationCardModel.ErrorCode,
                            Glow = operationCardModel.Glow,
                            Photo = operationCardModel.Photo,
                            UserID = operationCardModel.UserID,
                            ProductionOrderID = productionOrderID,
                            InstructionID = instructionID
                        };

                        dbSession.Set<OperationCard>().Add(operationCard);

                        // Persist to DB and fetch created PK (OperationCardID)
                        await dbSession.SaveChangesAsync();
                    }
                    else
                    {
                        // (Update)
                        operationCard.SlagNumber = operationCardModel.SlagNumber;
                        operationCard.BlockSurfaceID = operationCardModel.BlockSurface?.ID;
                        operationCard.ErrorCode = operationCardModel.ErrorCode;
                        operationCard.Glow = operationCardModel.Glow;
                        operationCard.Photo = operationCardModel.Photo;
                        operationCard.UserID = operationCardModel.UserID;
                    }

                    await SaveSignatures(operationCardModel, dbSession, operationCard.ID);
                    await SaveSubstances(operationCardModel, dbFacade, dbSession, operationCard);

                    // Persist Changes in the DataBase
                    await dbSession.SaveChangesAsync();

                    return null;
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }

        private async Task SaveSignatures(OperationCardModel operationCardModel, IDbSession dbSession, int operationCardId)
        {
            var ids = (operationCardModel.Blocks ?? new List<OperationCardModel.Block>()).Select(x => x.LogPKID)
                .Concat((operationCardModel.Grindings ?? new List<OperationCardModel.Grinding>()).Select(x => x.LogPKID))
                .Concat((operationCardModel.ISTs ?? new List<OperationCardModel.IST>()).Select(x => x.LogPKID))
                .ToArray();

            var existingIds = await dbSession.Set<OperationCardLogs>()
                .Where(x => ids.Contains(x.ID))
                .Select(x => x.ID)
                .ToArrayAsync();

            foreach (var item in operationCardModel.Blocks)
            {
                if (existingIds.Any(x => x == item.LogPKID))
                { continue; }

                var operationCardLog = new OperationCardLogs()
                {
                    ID = item.LogPKID,
                    OperationCardID = operationCardId,
                    Type = "Block",
                    Content = item.Weight,
                    LoggedError = null,
                    Date = item.Date,
                    OperatorID = item.Operator.ID
                };

                dbSession.Set<OperationCardLogs>().Add(operationCardLog);
            }

            foreach (var item in operationCardModel.Grindings)
            {
                if (existingIds.Any(x => x == item.LogPKID))
                { continue; }

                var operationCardLog = new OperationCardLogs()
                {
                    ID = item.LogPKID,
                    OperationCardID = operationCardId,
                    Type = "Grinding",
                    Content = item.GrindingTime.ToString(),
                    LoggedError = item.Error,
                    Date = item.Date,
                    OperatorID = item.Operator.ID
                };

                dbSession.Set<OperationCardLogs>().Add(operationCardLog);
            }

            foreach (var item in operationCardModel.ISTs)
            {
                if (existingIds.Any(x => x == item.LogPKID))
                { continue; }

                var operationCardLog = new OperationCardLogs()
                {
                    ID = item.LogPKID,
                    OperationCardID = operationCardId,
                    Type = "IST",
                    Content = item.Ist,
                    LoggedError = null,
                    Date = item.Date,
                    OperatorID = item.Operator.ID
                };

                dbSession.Set<OperationCardLogs>().Add(operationCardLog);
            }
        }

        private async Task SaveSubstances(OperationCardModel operationCardModel, IDbFacade dbFacade, IDbSession dbSession, OperationCard operationCard)
        {
            if (operationCardModel.Substances.Operator == null && operationCardModel.Substances.SubstanceList == null) return;
            if (operationCardModel.Substances.SubstanceList.Count == 0) return;

            var operationCardLogSubst = new OperationCardLogs()
            {
                ID = operationCardModel.Substances.LogPKID,
                OperationCardID = operationCard.ID,
                Type = "Substances",
                Content = null,
                LoggedError = null,
                Date = operationCardModel.Substances != null ? operationCardModel.Substances.Date : new DateTime(),
                OperatorID = operationCardModel.Substances != null ? operationCardModel.Substances.Operator.ID : -1
            };

            // Substance TimeStamp
            if (operationCardLogSubst.OperatorID != -1)
            {
                if (!await dbSession.Set<OperationCardLogs>().AnyAsync(u => u.ID == operationCardLogSubst.ID))
                {
                    dbSession.Set<OperationCardLogs>().Add(operationCardLogSubst);
                }      
                else
                {
                    var logEntry = await dbSession.Set<OperationCardLogs>().FirstOrDefaultAsync(u => u.ID == operationCardLogSubst.ID);
                    logEntry.Date = operationCardModel.Substances.Date;
                    logEntry.OperatorID = operationCardModel.Substances.Operator.ID;
                }
            }

            // Substances
            if (operationCardModel.Substances?.SubstanceList != null)
            {
                // Get Instruction Recipe Materials
                var instructionRecipeMaterials = new GetAllSubstances(dbFacade).Execute(operationCardModel.HeatNumber);

                // Get Instruction
                var instruction = dbSession.Set<Instruction>()
                    .Include(x => x.Materials)
                    .Where(x => x.ID == operationCard.InstructionID)
                    .FirstOrDefault();

                foreach (var item in operationCardModel.Substances?.SubstanceList.Where(x => x.Weight > 0))
                {
                    // Add or Update to the newest Amounts
                    if (instruction.Materials.Any(y => y.MaterialID == item.Subst.ID))
                        instruction.Materials.FirstOrDefault(y => y.MaterialID == item.Subst.ID).Amount = item.Weight;
                    else
                        instruction.Materials.Add(new InstructionMaterial() {
                            InstructionID = instruction.ID,
                            MaterialID = item.Subst.ID,
                            Amount = item.Weight
                        });
                }
            }
        }
    }
}
