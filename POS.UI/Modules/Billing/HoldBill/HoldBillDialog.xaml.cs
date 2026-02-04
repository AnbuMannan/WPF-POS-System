using System.Windows;
using POS.UI.Core.Services;
using POS.UI.Modules.Billing.BillingScreen;
using MessageBox = System.Windows.MessageBox;

namespace POS.UI.Modules.Billing.HoldBill
{
    public partial class HoldBillDialog : Window
    {
        private readonly BillingApiService _billingApi;
        private readonly BillingViewModel _billingViewModel;

        public HoldBillDialog(BillingApiService billingApi, BillingViewModel billingViewModel)
        {
            InitializeComponent();
            _billingApi = billingApi;
            _billingViewModel = billingViewModel;
            
            DataContext = new { Summary = $"Items: {billingViewModel.CartItems.Count}, Total: ₹{billingViewModel.GrandTotal:N2}" };
        }

        private async void HoldBill_Click(object sender, RoutedEventArgs e)
        {
            var reason = ReasonTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Please enter a reason for holding the bill.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var cartJson = _billingViewModel.GetCartJson();
                var result = await _billingApi.HoldBillAsync(reason, cartJson, _billingViewModel.GrandTotal);
                
                if (result != null)
                {
                    MessageBox.Show($"Bill held successfully! (ID: {result.Id})", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _billingViewModel.ClearCartCommand.Execute(null);
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to hold bill: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
