using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace SIBS.MBWAY.Windows.Converter
{
    public sealed class TextColorSelectionToForegroundColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                ViewModels.WithdrawalViewModel.WithdrawalTile withdrawalTile = (ViewModels.WithdrawalViewModel.WithdrawalTile)value;

                bool isSelected = withdrawalTile.IsSelected;

                return isSelected
                    ? App.Current.Resources["MbWayWhite"] as SolidColorBrush
                    : App.Current.Resources["MbWayColorRed"] as SolidColorBrush;
            }
            catch (Exception) { }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
