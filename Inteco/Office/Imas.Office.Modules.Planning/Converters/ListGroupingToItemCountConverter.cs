using Imas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(IEnumerable<object>), typeof(int))]
    public class ListGroupingToItemCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<object> groupedOrders = (IEnumerable<object>)value;
            int totalProductionOrders = 0;

            foreach (ProductionOrder order in groupedOrders)
            {
                totalProductionOrders += order.Quantity;
            }

            return totalProductionOrders;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
