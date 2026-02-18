using POS.UI.Core.Services;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Customers.Loyalty;

public partial class LoyaltySettingsView : UserControl
{
    public LoyaltySettingsView()
    {
        InitializeComponent();

        if (App.ServiceProvider != null)
        {
            var service = App.ServiceProvider.GetService(typeof(LoyaltyApiService)) as LoyaltyApiService;
            if (service != null)
            {
                DataContext = new LoyaltySettingsViewModel(service);
            }
        }
    }
}
