using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Core.Navigation
{
    public static class ViewResolver
    {
        public static UserControl Resolve(string viewName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var viewType = assembly.GetTypes()
                .FirstOrDefault(t =>
                    typeof(UserControl).IsAssignableFrom(t) &&
                    t.Name.Equals(viewName, StringComparison.OrdinalIgnoreCase));

            if (viewType == null)
                throw new Exception($"View not registered: {viewName}");

            return (UserControl)Activator.CreateInstance(viewType);
        }
    }
}
