using System.Windows;
using POS.UI.Core.Services;
using POS.UI.Modules.Billing.BillingScreen;

namespace POS.UI.Modules.Billing.HeldBills
{
    public partial class HeldBillsListView : Window
    {
        public HeldBillsListViewModel ViewModel { get; }

        public HeldBillsListView(BillingApiService billingApi, BillingViewModel billingViewModel)
        {
            InitializeComponent();
            ViewModel = new HeldBillsListViewModel(billingApi, billingViewModel);
            DataContext = ViewModel;
            Loaded += async (s, e) => await ViewModel.LoadHeldBillsAsync();
        }
    }
}
