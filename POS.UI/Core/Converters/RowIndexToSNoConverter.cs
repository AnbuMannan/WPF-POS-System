using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace POS.UI.Core.Converters
{
    public class RowIndexToSNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGridRow row && row.GetIndex() >= 0)
            {
                return row.GetIndex() + 1;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
