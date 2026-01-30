using System;
using System.Globalization;
using System.Windows.Data;
using POS.Shared.Models;

namespace POS.UI.Core.Converters
{
    public class SearchMatchConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            var product = values[0] as ProductDto;
            var searchText = values[1] as string;

            if (product == null || string.IsNullOrWhiteSpace(searchText))
                return false;

            searchText = searchText.ToLower();

            return
                (!string.IsNullOrEmpty(product.Name) && product.Name.ToLower().Contains(searchText)) ||
                (!string.IsNullOrEmpty(product.SKU) && product.SKU.ToLower().Contains(searchText)) ||
                (!string.IsNullOrEmpty(product.Barcode) && product.Barcode.ToLower().Contains(searchText));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
