using POS.UI.Core.Services;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Reports.EODReport
{
    public partial class EODReportView : UserControl
    {
        public EODReportView()
        {
            InitializeComponent();
            var service = App.ServiceProvider?.GetService(typeof(EODReportApiService)) as EODReportApiService;
            var printService = App.ServiceProvider?.GetService(typeof(IPrintService)) as IPrintService;
            var pdfExportService = App.ServiceProvider?.GetService(typeof(PdfExportService)) as PdfExportService;

            if (service != null && printService != null && pdfExportService != null)
            {
                DataContext = new EODReportViewModel(service, printService, pdfExportService);
            }
        }
    }
}
