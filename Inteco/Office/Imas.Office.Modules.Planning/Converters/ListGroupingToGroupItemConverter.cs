using Imas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(object[]), typeof(ProductionOrder))]
    public class ListGroupingToGroupItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is IEnumerable<object> groupedOrders && values[1] is ObservableCollection<ProductionOrder> plannerOrders)
            {
                int totalProductionOrders = 0;
                ProductionOrder groupRepresentativeOrder = null;
                foreach (ProductionOrder order in groupedOrders)
                {
                    if (groupRepresentativeOrder == null)
                        groupRepresentativeOrder = order.Copy(); // First order from the groupped items

                    totalProductionOrders += order.Quantity;
                }

                if(groupRepresentativeOrder != null)
                {
                    groupRepresentativeOrder.NotGroupHeader = false;
                    groupRepresentativeOrder.Quantity = totalProductionOrders;

                    groupRepresentativeOrder.ChargesNumber = plannerOrders.Where(x => x.MotherHeat.Name == groupRepresentativeOrder.MotherHeat.Name).Count();
                }

                return groupRepresentativeOrder;
            }
            
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
