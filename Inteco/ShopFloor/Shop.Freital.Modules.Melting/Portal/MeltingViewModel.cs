using System;
using System.Linq;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Imas.Mvvm;
using Imas.LocExtension;
using Imas.Mvvm.Bindables;
using Shop.Common;
using Shop.Business.Queries;
using Shop.Common.ViewModels;
using Shop.Freital.Modules.Melting.Widgets;

namespace Shop.Freital.Modules.Melting.Portal
{
    public class MeltingViewModel : ExplorerBindableBase
    {
        #region Members
        public const string Tag = "MeltingProductionOrders";

        private ObservableCollection<ProductionOrderViewModel> AllEntries;
        private readonly DispatcherTimer _dispatcherTimer;

        private readonly ObservableCollection<string> operationCardTooltipMessages = new ObservableCollection<string>(new string[2]);
        private readonly ObservableCollection<string> dataSheetTooltipMessages = new ObservableCollection<string>(new string[2]);
        #endregion

        #region Properties
        public override string ListActivity => Tag;
        public LiveChartWidgetViewModel LiveChartWidget { get; }
        public EventsWidgetViewModel EventsWidget { get; }
        public ObservableCollection<ProductionOrderViewModel> Entries { get; private set; }
        public bool ShowChartAndEventsSection { get; private set; }

        public ObservableCollection<string> DataSheetTooltipMessages
        {
            get
            {
                dataSheetTooltipMessages[0] = (string)new TranslationData("TooltipElectrodeNotPrepared").Value;
                dataSheetTooltipMessages[1] = (string)new TranslationData("TooltipElectrodePrepared").Value;
                return dataSheetTooltipMessages;
            }
        }

        public ObservableCollection<string> OperationCardTooltipMessages
        {
            get
            {
                operationCardTooltipMessages[0] = (string)new TranslationData("TooltipNoAdditionalData").Value;
                operationCardTooltipMessages[1] = (string)new TranslationData("TooltipAdditionalData").Value;
                return operationCardTooltipMessages;
            }
        }
        #endregion

        public MeltingViewModel(IModelFacade facade)
            : base(facade)
        {
            SetActiveProductionOrders();

            LiveChartWidget = new LiveChartWidgetViewModel(facade);
            EventsWidget = new EventsWidgetViewModel(facade);

            _dispatcherTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(AppSettings.RefreshTime)
            };

            _dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        public override void OnLeave()
        {
            _dispatcherTimer.Stop();
            base.OnLeave();
        }

        public void FilterDisplayedPlant(string plantName)
        {
            _dispatcherTimer.Stop();

            Entries = new ObservableCollection<ProductionOrderViewModel>(AllEntries.Where(m => m.PlantName == plantName));
            ShowChartAndEventsSection = Entries.Count > 0;

            LiveChartWidget.SetDataPointsByPlantInit(plantName, Entries.Count > 0 ? Entries.First().PlantIngotId : null);
            
            EventsWidget.SetDataByPlant(Entries.Count > 0 ? Entries.First().PlantIngotId : null);

            if (ShowChartAndEventsSection)
            {
                _dispatcherTimer.Start();
            }

            RaisePropertyChanged("Entries");
            RaisePropertyChanged("ShowChartAndEventsSection");
        }

        public bool IsMelting(string plantName)
        {
            return AllEntries.Any(m => m.PlantName == plantName);
        }

        /// <summary>
        /// Event to update Trend Data & Events every X ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            LiveChartWidget.UpdateDataPoints();
            EventsWidget.UpdateData();
        }

        private void SetActiveProductionOrders()
        {
            var entries = facade.Business()
                .Query<GetPlantActiveProductionOrders>()
                .Execute();

            AllEntries = new ObservableCollection<ProductionOrderViewModel>(entries);
            Entries = new ObservableCollection<ProductionOrderViewModel>();
        }
    }
}
