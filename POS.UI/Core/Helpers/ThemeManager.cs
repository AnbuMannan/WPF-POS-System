using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace POS.UI.Core.Helpers
{
    public static class ThemeManager
    {
        public static void ApplyTheme(string themeName)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri($"Themes/{themeName}.xaml", UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries[0] = dict;
        }
    }

}