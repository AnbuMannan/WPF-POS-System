using System.Windows;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Billing.QuickSale
{
    public partial class QuickSaleDialog : Window
    {
        public QuickSaleDialog()
        {
            InitializeComponent();
        }

        public QuickSaleDialog(BillingApiService billingApi, ProductApiService productApi, TaxProfileApiService taxProfileApi, UomApiService uomApi)
            : this()
        {
            // Store or use services as needed for quick sale
        }
    }
}
