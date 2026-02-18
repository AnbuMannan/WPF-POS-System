using POS.UI.Core.Services;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Reports.Finance
{
    public partial class ProfitLossReportView : UserControl
    {
        public ProfitLossReportView()
        {
            InitializeComponent();
            var api = App.ServiceProvider?.GetService(typeof(ReportApiService)) as ReportApiService;
            var printService = App.ServiceProvider?.GetService(typeof(IPrintService)) as IPrintService;
            DataContext = new ProfitLossReportViewModel(api!, printService);
        }
    }
}

