using Imas.Domain.Entities;
using System;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(ProductionOrder), typeof(Uri))]
    public class OperationCardToIconConverter : IValueConverter
    {
        private readonly Uri IconDefault = new Uri("../Icons/operation_card.png", UriKind.Relative);
        private readonly Uri IconChecked = new Uri("../Icons/operation_card_check.png", UriKind.Relative);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Uri operationCardIcon = null;

            if (value is ProductionOrder order)
            {
                if (order.HasAdditionalData)
                    operationCardIcon = IconChecked;
                else
                    operationCardIcon = IconDefault;
            }

            return operationCardIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
