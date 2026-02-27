namespace POS.UI.Modules.Diagnostics
{
    public partial class HealthView : System.Windows.Controls.UserControl
    {
        public HealthView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService(typeof(HealthViewModel)) as HealthViewModel;
        }
    }
}
