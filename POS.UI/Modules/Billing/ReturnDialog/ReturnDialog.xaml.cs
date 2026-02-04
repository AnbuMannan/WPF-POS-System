using System.Windows;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Billing.ReturnDialog
{
    public partial class ReturnDialog : Window
    {
        public ReturnDialog()
        {
            InitializeComponent();
        }

        public ReturnDialog(ReturnApiService returnApi, ProductApiService productApi, TaxProfileApiService taxProfileApi, UomApiService uomApi)
            : this()
        {
            // Store or use services as needed for returns
        }
    }
}
