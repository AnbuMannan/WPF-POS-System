using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Admin.Uom
{
    public partial class UomView : UserControl
    {
        public UomView()
        {
            InitializeComponent();
            var service = App.ServiceProvider?.GetService(typeof(UomApiService)) as UomApiService;
            DataContext = new UomViewModel(service);
        }
    }
}
