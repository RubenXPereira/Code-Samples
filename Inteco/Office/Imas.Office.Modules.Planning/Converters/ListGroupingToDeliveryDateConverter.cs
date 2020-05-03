using Imas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(IEnumerable<object>), typeof(int))]
    public class ListGroupingToDeliveryDateConverter : IValueConverter
    {
        // REGEX for the DeliveryDate Format as described in the Interface Doc. E.g "KW10 / 19"
        // private static readonly Regex dateRegex = new Regex(@"^[kK][wW]\s*(\d{2})\s*[\\\/]\s*(\d{2})$");

        // REGEX for the actual DeliveryDate Format provided. E.g. "45.KW/18"
        private static readonly Regex dateRegex = new Regex(@"^(\d{2}).[kK][wW]\s*[\\\/]\s*(\d{2})$");
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<object> groupedOrders = (IEnumerable<object>)value;
            string latestProductionOrderDeliveryWeek = string.Empty;
            int currentDeliveryWeek = 0, currentDeliveryYear = DateTime.Now.Year - 2000;

            foreach (ProductionOrder order in groupedOrders)
            {
                if (order.DeliveryDate == null) continue;

                string trimmedDeliveryDate = order.DeliveryDate.Trim();
                if (dateRegex.IsMatch(trimmedDeliveryDate))
                {
                    Match deliveryWeek = dateRegex.Match(trimmedDeliveryDate);
                    
                    int orderDeliveryYear = deliveryWeek.Groups.Count == 3 ? int.Parse(deliveryWeek.Groups[2].Value) : 0;
                    int orderDeliveryWeek = deliveryWeek.Groups.Count == 3 ? int.Parse(deliveryWeek.Groups[1].Value) : 0;

                    if ( (orderDeliveryYear > currentDeliveryYear && orderDeliveryWeek > 0) || (orderDeliveryYear == currentDeliveryYear && orderDeliveryWeek > currentDeliveryWeek))
                        latestProductionOrderDeliveryWeek = trimmedDeliveryDate;
                }
            }

            return latestProductionOrderDeliveryWeek;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
