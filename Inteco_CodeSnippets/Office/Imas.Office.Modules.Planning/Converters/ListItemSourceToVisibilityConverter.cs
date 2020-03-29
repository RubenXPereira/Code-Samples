using Imas.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(ObservableCollection<ProductionOrder>), typeof(Visibility))]
    public class ListItemSourceToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<ProductionOrder> orders = (ObservableCollection<ProductionOrder>)value;
            
            if(orders != null && orders.Count > 0)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
