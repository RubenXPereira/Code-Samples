using Imas.Domain.Entities;
using System;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(ProductionOrder), typeof(Uri))]
    public class DataSheetToIconConverter : IValueConverter
    {
        private readonly Uri IconDefault = new Uri("../Icons/datasheet.png", UriKind.Relative);
        private readonly Uri IconChecked = new Uri("../Icons/datasheet_check.png", UriKind.Relative);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Uri dataSheetIcon = null;

            if (value is ProductionOrder order)
            {
                switch (order.ProductionOrderStatus.Name)
                {
                    case ProductionOrderStatus.Planned:
                    case ProductionOrderStatus.Production:
                        dataSheetIcon = IconChecked;
                        break;
                        
                    default:
                        dataSheetIcon = IconDefault;
                        break;
                }
            }

            return dataSheetIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
