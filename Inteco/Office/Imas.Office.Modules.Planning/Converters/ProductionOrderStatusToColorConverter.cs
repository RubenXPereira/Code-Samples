using Imas.Domain.Entities;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(ProductionOrder), typeof(SolidColorBrush))]
    public class ProductionOrderStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush mappedColor = Styles.Colors.WhiteColor;

            if (value is ProductionOrder order)
            {
                switch (order.ProductionOrderStatus.Name)
                {
                    case ProductionOrderStatus.Production:
                        mappedColor = Styles.Colors.GreenColor;
                        break;

                    case ProductionOrderStatus.Planned:
                        mappedColor = Styles.Colors.YellowColor;
                        break;

                    case ProductionOrderStatus.Blocked:
                        mappedColor = Styles.Colors.RedColor;
                        break;
                }
            }

            return mappedColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
