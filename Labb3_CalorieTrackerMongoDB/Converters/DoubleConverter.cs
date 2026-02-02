using System;
using System.Globalization;
using System.Windows.Data;

namespace Labb3_CalorieTrackerMongoDB.Converters
{
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            if (value is double d)
                return d.ToString(CultureInfo.InvariantCulture); 
            return value.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (value as string)?.Trim();
            if (string.IsNullOrWhiteSpace(str))
                return 0.0;

           
            str = str.Replace(',', '.');

            if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return Binding.DoNothing;
        }
    }
}