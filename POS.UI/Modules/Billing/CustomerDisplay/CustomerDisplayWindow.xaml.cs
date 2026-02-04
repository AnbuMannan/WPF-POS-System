using System.Windows;
using POS.UI.Modules.Billing.BillingScreen;

namespace POS.UI.Modules.Billing.CustomerDisplay
{
    public partial class CustomerDisplayWindow : Window
    {
        public CustomerDisplayWindow()
        {
            InitializeComponent();
        }

        public void SetSource(BillingViewModel billingViewModel)
        {
            DataContext = billingViewModel;
        }
    }
}
