using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace POS.UI.Core.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSuccess && parameter is string param)
            {
                var parts = param.Split('|');
                if (parts.Length == 2)
                {
                    // For success case (true), use first color
                    // For failure case (false), use second color
                    var colorName = isSuccess ? parts[0] : parts[1];
                    
                    // Try to get the brush from application resources
                    var brush = System.Windows.Application.Current.TryFindResource($"Brush{colorName}") as SolidColorBrush;
                    
                    // Fallback to predefined colors if resource not found
                    return brush ?? (isSuccess 
                        ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96))  // Green for success
                        : new SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60))); // Red for danger
                }
            }
            
            // Default fallback
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}