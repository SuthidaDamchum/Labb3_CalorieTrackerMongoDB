using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Labb3_CalorieTrackerMongoDB.Converters
{
    public class MacroDiffToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int actual = (int)value;
            int goal = parameter is int g ? g : 0;
            return actual <= goal
                ? (SolidColorBrush)App.Current.Resources["BadgeGreenBg"]
                : (SolidColorBrush)App.Current.Resources["BadgeRedBg"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}