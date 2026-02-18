using System.Windows;
using System.Windows.Controls;

namespace POS.UI.Modules.Inventory.LabelPrinting;

/// <summary>
/// Interaction logic for LabelPrintingView.xaml
/// </summary>
public partial class LabelPrintingView : System.Windows.Controls.UserControl
{
    public LabelPrintingView()
    {
        InitializeComponent();
    }

    private void LabelPrintingView_Loaded(object sender, RoutedEventArgs e)
    {
        // Focus on search box when view loads
        ProductSearchBox?.Focus();
        // Ensure the control can receive keyboard input
        this.Focusable = true;
        this.Focus();
    }

    private void LabelPrintingView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (DataContext is LabelPrintingViewModel vm)
        {
            // Handle F2 for print
            if (e.Key == System.Windows.Input.Key.F2)
            {
                _ = vm.PrintLabelsAsync();
                e.Handled = true;
                return;
            }
            vm.HandleKeyDown(e);
        }
    }

    private async void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LabelPrintingViewModel vm)
        {
            await vm.PrintLabelsAsync();
        }
    }
}
