using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace POS.UI.Core.Converters
{
    public class VarianceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal val)
            {
                if (val > 0) return System.Windows.Media.Brushes.Green;
                if (val < 0) return System.Windows.Media.Brushes.Red;
            }
            return System.Windows.Media.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}