using System.Windows;
using POS.UI.Core.Services;
using POS.UI.Modules.Billing.BillingScreen;
using MessageBox = System.Windows.MessageBox;

namespace POS.UI.Modules.Billing.DraftBill
{
    public partial class DraftBillDialog : Window
    {
        private readonly BillingApiService _billingApi;
        private readonly BillingViewModel _billingViewModel;

        public DraftBillDialog(BillingApiService billingApi, BillingViewModel billingViewModel)
        {
            InitializeComponent();
            _billingApi = billingApi;
            _billingViewModel = billingViewModel;
            
            DataContext = new { Summary = $"Items: {billingViewModel.CartItems.Count}, Total: ₹{billingViewModel.GrandTotal:N2}" };
        }

        private async void SaveDraft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var draftName = DraftNameTextBox.Text?.Trim();
                var cartJson = _billingViewModel.GetCartJson();
                var result = await _billingApi.SaveDraftAsync(draftName, _billingViewModel.BillNumber, cartJson, _billingViewModel.GrandTotal);
                
                if (result != null)
                {
                    MessageBox.Show($"Draft saved successfully! (ID: {result.Id})", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _billingViewModel.ClearCartCommand.Execute(null);
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save draft: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
