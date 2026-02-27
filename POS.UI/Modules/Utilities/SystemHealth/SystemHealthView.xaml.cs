namespace POS.UI.Modules.Utilities.SystemHealth
{
    public partial class SystemHealthView : System.Windows.Controls.UserControl
    {
        public SystemHealthView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService(typeof(SystemHealthViewModel)) as SystemHealthViewModel;
        }
    }
}
