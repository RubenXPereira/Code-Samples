using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(object[]), typeof(string))]
    public class OperationCardToTooltipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string dataSheetTooltip = null;

            if (values.Length == 2 && values[0] is bool hasAdditionalData && values[1] is ObservableCollection<string> labels)
            {
                if(hasAdditionalData)
                    dataSheetTooltip = (labels as ObservableCollection<string>)[1];
                else
                    dataSheetTooltip = (labels as ObservableCollection<string>)[0];
            }

            return dataSheetTooltip;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
