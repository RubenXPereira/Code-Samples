using Imas.Domain.Entities;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imas.Office.Modules.Planning.Dialogs
{
    public class AssignProductionOrderToPlantDialogViewModel : BindableBase
    {
        private string motherheat;
        private Plant plant;
        private int plantID;

        public AssignProductionOrderToPlantDialogViewModel(string motherheat, string assignedPlant, IEnumerable<Plant> plants, Action<AssignProductionOrderToPlantDialogViewModel> saveHandler, Action<AssignProductionOrderToPlantDialogViewModel> closeHandler)
        {
            Motherheat = motherheat;
            Plants = plants;

            if (assignedPlant != null)
                plant = Plants.Where(x => x.Name == assignedPlant)?.FirstOrDefault();

            CloseCommand = new DelegateCommand(() => closeHandler(this));
            SaveCommand = new DelegateCommand(() => saveHandler(this), () => Plant != null);
        }

        public DelegateCommand SaveCommand { get; set; }

        public DelegateCommand CloseCommand { get; set; }

        public string Motherheat
        {
            get => motherheat;
            set
            {
                SetProperty(ref motherheat, value);
            }
        }

        public Plant Plant
        {
            get => plant;
            set
            {
                SetProperty(ref plant, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
        
        public int PlantID
        {
            get => plantID;
            set
            {
                Plant = Plants.FirstOrDefault(p => p.ID == value);
                SetProperty(ref plantID, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public IEnumerable<Plant> Plants { get; set; }
    }
}
