using System;
using System.Windows.Controls;

namespace POS.UI.Modules.Billing.QuickSale
{
    public partial class QuickSaleView : System.Windows.Controls.UserControl
    {
        public QuickSaleView()
        {
            InitializeComponent();
            this.Loaded += QuickSaleView_Loaded;
        }

        private void QuickSaleView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is QuickSaleViewModel vm)
            {
                vm.RequestFocusToSearch = () => BarcodeSearchBox.Focus();
                vm.ShowReceiptPreview = (receipt) => 
                {
                    var printService = App.ServiceProvider?.GetService(typeof(POS.UI.Core.Services.IPrintService)) as POS.UI.Core.Services.IPrintService;
                    var emailService = App.ServiceProvider?.GetService(typeof(POS.UI.Core.Services.IEmailReceiptService)) as POS.UI.Core.Services.IEmailReceiptService;
                    var dialog = new POS.UI.Modules.Billing.ReceiptPreview.PrintPreviewDialog(receipt, printService, emailService);
                    dialog.Owner = System.Windows.Window.GetWindow(this);
                    dialog.ShowDialog();
                };
                BarcodeSearchBox.Focus();
            }
        }
    }
}