using POS.UI.Core.Services;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Reports.Sales
{
    public partial class SalesReportView : UserControl
    {
        public SalesReportView()
        {
            InitializeComponent();
            var api = App.ServiceProvider?.GetService(typeof(ReportApiService)) as ReportApiService;
            DataContext = new ReportViewerViewModel(api!, ReportType.SalesSummary);
        }
    }
}
