using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Imas.Mvvm;
using Imas.Mvvm.Bindables;
using Shop.Business.Queries;
using Shop.Common.ViewModels;
using Imas.Domain.Entities;

namespace Shop.Freital.Modules.Melting.Widgets
{
    public class EventsWidgetViewModel : ExplorerBindableBase
    {
        #region Members
        public const string Tag = "EventsWidgetViewModel";
        private DateTime _lastUpdate;
        private int? _currentIngotID;
        #endregion

        #region Properties
        public override string ListActivity => Tag;

        public ObservableCollection<MachineEventViewModel> Events { get; private set; }
        #endregion

        public EventsWidgetViewModel(IModelFacade facade)
            : base(facade)
        {
            Events = new ObservableCollection<MachineEventViewModel>();
        }

        public void SetDataByPlant(int? ingotId)
        {
            _currentIngotID = ingotId;
            _lastUpdate = _currentIngotID.HasValue ? ( GetIngot(_currentIngotID.Value).MeltstartTime ?? DateTime.Now) : DateTime.Now;

            var events = GetEvents(_currentIngotID, _lastUpdate);

            Events.Clear();
            Events.AddRange(events);
        }

        public void UpdateData()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var events = GetEvents(_currentIngotID, _lastUpdate)
                    ?.OrderBy(x => x.Timestamp)
                    .ToList();

                _lastUpdate = DateTime.Now;

                foreach (var machineEvent in events)
                { Events.Insert(0, machineEvent); }
            });
        }

        private Ingot GetIngot(int ingotID)
        {
            return facade.Business().Query<GetIngotByID>().Execute(ingotID);
        }

        private IList<MachineEventViewModel> GetEvents(int? ingotId, DateTime fromDate) =>
            facade.Business()
                .Query<GetMachineEventByPlant>()
                .Execute(ingotId, fromDate);
    }
}
