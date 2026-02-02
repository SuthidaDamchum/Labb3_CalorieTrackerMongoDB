using System;
using System.Globalization;
using System.Windows.Data;

namespace Labb3_CalorieTrackerMongoDB.Converters
{
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString() ?? "";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrWhiteSpace(str))
                return 0.0;
       
            if (double.TryParse(str.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            if (double.TryParse(str.Replace('.', ','), NumberStyles.Any, CultureInfo.GetCultureInfo("sv-SE"), out result))
                return result;
            return 0.0;
        }
    }
}

