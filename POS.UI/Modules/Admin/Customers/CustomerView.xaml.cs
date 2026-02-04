using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Admin.Customers
{
    public partial class CustomerView : UserControl
    {
        public CustomerView()
        {
            InitializeComponent();
            DataContext = new CustomerViewModel(App.ServiceProvider.GetService(typeof(CustomerApiService)) as CustomerApiService);
        }
    }
}
