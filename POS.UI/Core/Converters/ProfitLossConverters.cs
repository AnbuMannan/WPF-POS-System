using System;
using System.Globalization;
using System.Windows.Data;

namespace POS.UI.Core.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = parameter as string ?? string.Empty;
            var parts = param.Split('|');
            var trueText = parts.Length > 0 ? parts[0] : "True";
            var falseText = parts.Length > 1 ? parts[1] : "False";

            if (value is bool flag)
            {
                return flag ? trueText : falseText;
            }

            return falseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SegmentWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3)
                return 0d;

            if (values[0] is not decimal amountDecimal)
                return 0d;

            if (values[1] is not decimal maxDecimal || maxDecimal <= 0)
                return 0d;

            if (values[2] is not double availableWidth || availableWidth <= 0)
                return 0d;

            var amount = (double)amountDecimal;
            var max = (double)maxDecimal;

            var ratio = amount / max;
            if (ratio < 0) ratio = 0;
            if (ratio > 1) ratio = 1;

            var width = availableWidth * ratio * 0.9;
            if (width < 20)
                width = 20;

            return width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
