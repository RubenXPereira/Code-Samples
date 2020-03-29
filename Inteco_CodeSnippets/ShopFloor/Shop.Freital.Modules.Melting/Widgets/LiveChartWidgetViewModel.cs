using System;
using OxyPlot;
using System.Linq;
using OxyPlot.Axes;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Imas.Mvvm;
using Imas.Mvvm.Bindables;
using Shop.Common;
using Shop.Business.Queries;
using Shop.Common.ViewModels;
using Imas.Domain.Entities;

namespace Shop.Freital.Modules.Melting.Widgets
{
    public class LiveChartWidgetViewModel : ExplorerBindableBase
    {
        #region Members
        public const string Tag = "LiveChartWidgetViewModel";
        private readonly int MachineDataBatchSize = 10000;
        private string _plantName;
        private List<int> _availableProcessIDs;
        private int _startID;
        private int _currentIngotID;
        #endregion

        #region Properties
        public override string ListActivity => Tag;
        public bool ShowPlot { get; private set; }
        public Dictionary<string, ObservableCollection<DataPoint>> DataSeries { get; }
        #endregion

        public LiveChartWidgetViewModel(IModelFacade facade)
            : base(facade)
        {
            DataSeries = new Dictionary<string, ObservableCollection<DataPoint>>
            {
                { Constants.MELTING_AXIS_CURRENT, new ObservableCollection<DataPoint>() },
                { Constants.MELTING_AXIS_VOLTAGE, new ObservableCollection<DataPoint>() },
                { Constants.MELTING_AXIS_POWER, new ObservableCollection<DataPoint>() },
                { Constants.MELTING_AXIS_RESISTANCE, new ObservableCollection<DataPoint>() },
                { Constants.MELTING_AXIS_MELTRATE, new ObservableCollection<DataPoint>() },
                { Constants.MELTING_AXIS_SWING, new ObservableCollection<DataPoint>() },
                { Constants.MELTING_AXIS_ELECTRODE_POSITION, new ObservableCollection<DataPoint>() },
                { Constants.MELTING_AXIS_ELECTRODE_WEIGHT, new ObservableCollection<DataPoint>() },
                { Constants.MELTING_AXIS_INGOT_WEIGHT, new ObservableCollection<DataPoint>() }
            };

            _startID = 0;
        }

        public void SetDataPointsByPlantInit(string plantName, int? ingotID)
        {
            if (ingotID.HasValue)
            {
                _plantName = plantName;
                _currentIngotID = ingotID.Value;

                int? startID = GetIngot(_currentIngotID).MachineDataStartId;
                if (startID.HasValue)
                {
                    _startID = startID.Value;

                    _availableProcessIDs = GetProcessVariablesByPlant(plantName);

                    // Get already existing machine data for this Ingot
                    var machineData = GetMachineData(_currentIngotID, _startID, _availableProcessIDs);

                    if (machineData != null && machineData.Item1.Count > 0)
                    {
                        _startID = machineData.Item2 + 1;

                        // Plot the data
                        ShowPlot = true;
                        RaisePropertyChanged("ShowPlot");

                        foreach (var target in DataSeries)
                        {
                            if (ShowPlot)
                            { FillSeries(target.Value, GetDataPoints(_plantName, target.Key, machineData.Item1)); }
                            else
                            { target.Value.Clear(); }
                        }

                        return;
                    }
                }
            }
            ShowPlot = false;
        }

        public void UpdateDataPoints()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var machineData = GetMachineData(_currentIngotID, _startID, _availableProcessIDs);

                if (machineData != null && machineData.Item1.Count > 0)
                {
                    _startID = machineData.Item2 + 1;

                    if (!ShowPlot)
                    {
                        ShowPlot = true;
                        RaisePropertyChanged("ShowPlot");
                    }

                    foreach (var target in DataSeries)
                    {
                        var dataPoints = GetDataPoints(_plantName, target.Key, machineData.Item1);

                        foreach (var data in dataPoints)
                        { target.Value.Add(DateTimeAxis.CreateDataPoint(data.Timestamp, data.Value)); }
                    }
                }
            });
        }

        private void FillSeries(ObservableCollection<DataPoint> target, IList<DataPointViewModel> dataPoints)
        {
            target.Clear();

            foreach (var data in dataPoints)
            { target.Add(DateTimeAxis.CreateDataPoint(data.Timestamp, data.Value)); }
        }

        private IList<DataPointViewModel> GetDataPoints(string plantName, string target, IList<MachineDataViewModel> machineData)
        {
            Func<MachineDataViewModel, bool> exp = null;

            switch (target)
            {
                case Constants.MELTING_AXIS_CURRENT:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_CURRENT;
                    break;
                case Constants.MELTING_AXIS_ELECTRODE_POSITION:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_ELECTRODE_POSITION;
                    break;
                case Constants.MELTING_AXIS_ELECTRODE_WEIGHT:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_ELECTRODE_WEIGHT;
                    break;
                case Constants.MELTING_AXIS_INGOT_WEIGHT:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_INGOT_WEIGHT;
                    break;
                case Constants.MELTING_AXIS_MELTRATE:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_MELTRATE;
                    break;
                case Constants.MELTING_AXIS_POWER:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_POWER;
                    break;
                case Constants.MELTING_AXIS_RESISTANCE:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_RESISTANCE;
                    break;
                case Constants.MELTING_AXIS_SWING:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_SWING;
                    break;
                case Constants.MELTING_AXIS_VOLTAGE:
                    exp = x => x.ProcessVariableName == plantName + "." + Constants.PROCESS_VARIABLE_VOLTAGE;
                    break;
                default:
                    throw new Exception("Incorrect Axis Name");
            }

            return machineData.Where(exp).Select(x => x.Points).FirstOrDefault() ?? new List<DataPointViewModel>();
        }

        private Ingot GetIngot(int ingotID)
        {
            return facade.Business().Query<GetIngotByID>().Execute(ingotID);
        }

        private List<int> GetProcessVariablesByPlant(string plantName)
        {
            return facade.Business().Query<GetProcessVariablesByPlant>().Execute(plantName);
        }

        private Tuple<List<MachineDataViewModel>, int> GetMachineData(int ingotID, int fromID, List<int> availableProcessIDs) =>
            facade.Business()
                .Query<GetGroupedMachineDataByPlant>()
                .Execute(ingotID, fromID, MachineDataBatchSize, availableProcessIDs);
    }
}
