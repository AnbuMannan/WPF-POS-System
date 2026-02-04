using System.Windows;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Billing.PaymentDialog
{
    public partial class PaymentDialog : Window
    {
        public PaymentDialogViewModel ViewModel { get; }

        public PaymentDialog(BillingApiService billingApi, decimal totalAmount)
        {
            InitializeComponent();
            ViewModel = new PaymentDialogViewModel(totalAmount);
            ViewModel.RequestClose = () => DialogResult = true;
            DataContext = ViewModel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
