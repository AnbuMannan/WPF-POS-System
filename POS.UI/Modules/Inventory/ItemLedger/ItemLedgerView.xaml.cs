using System.Windows;
using System.Windows.Controls;

namespace POS.UI.Modules.Inventory.ItemLedger;

/// <summary>
/// Interaction logic for ItemLedgerView.xaml
/// </summary>
public partial class ItemLedgerView : System.Windows.Controls.UserControl
{
    public ItemLedgerView()
    {
        InitializeComponent();
    }

    private void ItemLedgerView_Loaded(object sender, RoutedEventArgs e)
    {
        // Focus on search box when view loads
        ProductSearchBox?.Focus();
    }

    private void ItemLedgerView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (DataContext is ItemLedgerViewModel vm)
        {
            vm.HandleKeyDown(e);
        }
    }
}
