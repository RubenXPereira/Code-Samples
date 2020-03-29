using System;
using System.Windows.Data;
using Imas.LocExtension;

namespace Imas.Office.Modules.Planning.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class PlantNameHackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string plantName && plantName != null)
            {
                // Hack for Group Header Alignment with GridView Header (TextBlock Control was not applying Margin Left nor Padding Left)
                return "     " + new TranslationData(plantName);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
