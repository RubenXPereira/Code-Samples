using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using SIBS.MBWAY.Windows.Utils;

namespace SIBS.MBWAY.Windows.Converter
{
    public sealed class CardImageBytesToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value != null)
                {
                    byte[] image = (byte[]) value;
                    return Helper.ConvertToBitmapImage(image).Result;
                }

                return new BitmapImage(new Uri("ms-appx:///Images/Card/cartao_default.png"));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
