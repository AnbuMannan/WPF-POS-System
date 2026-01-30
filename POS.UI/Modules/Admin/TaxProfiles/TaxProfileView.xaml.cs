using POS.UI.Core.Services;
using System.Windows.Controls;

namespace POS.UI.Modules.Admin.TaxProfiles
{
    public partial class TaxProfileView : UserControl
    {
        public TaxProfileView()
        {
            InitializeComponent();
            DataContext = new TaxProfileViewModel(App.ServiceProvider.GetService(typeof(TaxProfileApiService)) as TaxProfileApiService);
        }
    }
}
