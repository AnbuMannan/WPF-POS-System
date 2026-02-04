using System.Globalization;
using System.Windows.Data;

namespace POS.UI.Core.Converters
{
    public class TaxRateToBandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal taxRate)
            {
                if (taxRate == 0) return 0;
                if (taxRate <= 5) return 5;
                if (taxRate <= 12) return 12;
                if (taxRate <= 18) return 18;
                return 28;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
