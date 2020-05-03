using Imas.Domain.Entities;
using System;
using System.Windows;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(ProductionOrder), typeof(Visibility))]
    public class ProductionOrderStatusToInfoVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var iconVisibility = Visibility.Hidden;

            if (value is ProductionOrder order)
            {
                if(order.ProductionOrderStatus?.Name == ProductionOrderStatus.Blocked && order.NotGroupHeader)
                {
                    iconVisibility = Visibility.Visible;
                }
                else
                {
                    iconVisibility = Visibility.Hidden;
                }
            }

            return iconVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
