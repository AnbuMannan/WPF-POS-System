using System;
using System.Collections;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace POS.UI.Core.Converters;

/// <summary>
/// Converts Validation.Errors collection to the first error's ErrorContent string.
/// Returns empty string when collection is null or empty (avoids ArgumentOutOfRangeException).
/// </summary>
public class FirstValidationErrorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable errors)
        {
            foreach (var err in errors)
            {
                if (err is ValidationError ve && ve.ErrorContent != null)
                    return ve.ErrorContent.ToString();
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
