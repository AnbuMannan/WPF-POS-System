using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System.Windows.Controls;

namespace POS.UI.Modules.Admin.Brands
{
    public partial class BrandView : UserControl
    {
        public BrandView()
        {
            InitializeComponent();
            DataContext = new BrandViewModel(App.ServiceProvider.GetService(typeof(BrandApiService)) as BrandApiService);
        }
    }
}
