using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Customers.Outstanding;

public partial class CustomerOutstandingView : System.Windows.Controls.UserControl
{
    public CustomerOutstandingView()
    {
        InitializeComponent();
        Loaded += CustomerOutstandingView_Loaded;
    }

    private void CustomerOutstandingView_Loaded(object sender, RoutedEventArgs e)
    {
        SearchBox?.Focus();
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // Handle Ctrl+F to focus search
        if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            SearchBox?.Focus();
            SearchBox?.SelectAll();
            e.Handled = true;
        }
    }
}
