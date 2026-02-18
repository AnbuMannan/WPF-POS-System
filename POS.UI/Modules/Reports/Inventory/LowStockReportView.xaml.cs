using System.Windows;
using System.Windows.Controls;
using POS.Shared.Models;
using POS.UI.Core.Services;
using POS.UI.Modules.Inventory.ItemLedger;
using UserControl = System.Windows.Controls.UserControl;
using Button = System.Windows.Controls.Button;

namespace POS.UI.Modules.Reports.Inventory
{
    public partial class LowStockReportView : UserControl
    {
        public LowStockReportView()
        {
            InitializeComponent();
            var api = App.ServiceProvider?.GetService(typeof(ReportApiService)) as ReportApiService;
            DataContext = new LowStockReportViewModel(api!);
        }

        private async void LedgerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not LowStockItemRow row)
                return;

            var ledgerApi = App.ServiceProvider?.GetService(typeof(ItemLedgerApiService)) as ItemLedgerApiService;
            var productApi = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;

            if (ledgerApi == null || productApi == null)
            {
                POS.UI.Components.DialogService.Error("Item Ledger", "Required services are not available.");
                return;
            }

            var vm = new ItemLedgerViewModel(ledgerApi, productApi);
            await vm.PreselectProductAsync(row.ProductId);

            var view = new ItemLedgerView
            {
                DataContext = vm
            };

            var window = new Window
            {
                Title = $"Item Ledger - {row.ProductName}",
                Content = view,
                Width = 1200,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.ShowDialog();
        }
    }
}
