using System.Windows;
using POS.UI.Core.Services;
using POS.UI.Modules.Billing.BillingScreen;

namespace POS.UI.Modules.Billing.DraftBills
{
    public partial class DraftBillsListView : Window
    {
        public DraftBillsListViewModel ViewModel { get; }

        public DraftBillsListView(BillingApiService billingApi, BillingViewModel billingViewModel)
        {
            InitializeComponent();
            ViewModel = new DraftBillsListViewModel(billingApi, billingViewModel);
            DataContext = ViewModel;
            Loaded += async (s, e) => await ViewModel.LoadDraftBillsAsync();
        }
    }
}
