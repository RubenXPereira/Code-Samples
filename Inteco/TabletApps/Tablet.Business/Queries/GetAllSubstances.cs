using Imas.Persistence;
using Imas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Collections.ObjectModel;
using Imas.Domain.Interfaces;

namespace Imas.Business.Queries
{
    public class GetAllSubstances : AbstractQuery
    {
        public GetAllSubstances(IDbFacade dbFacade) 
            : base(dbFacade)
        { }

        public ObservableCollection<InstructionMaterial> Execute(string heatNumber)
        {
            try
            {
                // RecipeESR approach
                var materials = new GetAllMaterials(dbFacade).ExecuteWithMaterialTypeAndMaterialElements().Where(m => m.Active == true);

                using (var dbSession = dbFacade.Transaction())
                {
                    int? id = dbSession.Set<ProductionOrder>().Where(x => x.HeatNumber == heatNumber).FirstOrDefault()?.ID;

                    if (id == null) return null;

                    Instruction instruction = dbSession.Set<Instruction>()
                        .Include(x => x.ProductionOrder)
                        .Include(x => x.Materials)
                        .Include(r => r.Materials.Select(rr => rr.Material))
                        .Include(r => r.Materials.Select(rr => rr.Material.MaterialType))
                        .Include(r => r.Materials.Select(rr => rr.Material.MaterialElements.Select(x => x.Element)))
                        .FirstOrDefault(x => x.ProductionOrderID == id);

                    if (instruction != null)
                    {
                        return CreateMaterialList(instruction.Materials, materials, "Slag");
                    }

                    return null;
                }
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }

        public IList<Substance> ExecuteWrapper(string heatNumber)
        {
            try
            {
                var listSet = Execute(heatNumber);

                if(listSet != null)
                {
                    // Convert to List<Substance>
                    var substances = new List<Substance>();
                    foreach (var item in listSet)
                    {
                        substances.Add(new Substance()
                        {
                            ID = item.MaterialID,
                            Name = item.Material.Name,
                            Charge = item.Material.Number,
                            Amount = item.Amount
                        });
                    }

                    return substances;
                }
                
                return null;
            }
            catch (Exception e)
            { throw Oops.Rewrap(e); }
        }

        private ObservableCollection<InstructionMaterial> CreateMaterialList(IEnumerable<IRecipeMaterial> existingMaterials, IEnumerable<Material> allMaterials, string materialType)
        {
            ObservableCollection<InstructionMaterial> recipeMaterials = new ObservableCollection<InstructionMaterial>();

            foreach (var item in allMaterials?.Where(x => x.MaterialType.Name == materialType).OrderBy(p => p.Name))
            {
                IRecipeMaterial existingMaterial = existingMaterials.FirstOrDefault(x => x.MaterialID == item.ID);

                var newMaterial = new InstructionMaterial
                {
                    MaterialID = item.ID,
                    Material = item
                };

                if (existingMaterial != null && typeof(IRecipeMaterial).IsAssignableFrom(existingMaterial.GetType()))
                {
                    newMaterial.Amount = existingMaterial.Amount;
                    newMaterial.StartWeight = existingMaterial.StartWeight;
                    newMaterial.StopWeight = existingMaterial.StopWeight;
                }                

                recipeMaterials.Add(newMaterial);
            }

            return recipeMaterials;
        }
    }
}
