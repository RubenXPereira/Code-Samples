using Imas.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(object[]), typeof(string))]
    public class DataSheetToTooltipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string dataSheetTooltip = null;

            if (values.Length == 2 && values[0] is ProductionOrderStatus orderStatus && values[1] is ObservableCollection<string> labels)
            {
                switch (orderStatus.Name)
                {
                    case ProductionOrderStatus.Planned:
                    case ProductionOrderStatus.Production:
                        dataSheetTooltip = (labels as ObservableCollection<string>)[1];
                        break;

                    default:
                        dataSheetTooltip = (labels as ObservableCollection<string>)[0];
                        break;
                }
            }

            return dataSheetTooltip;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
