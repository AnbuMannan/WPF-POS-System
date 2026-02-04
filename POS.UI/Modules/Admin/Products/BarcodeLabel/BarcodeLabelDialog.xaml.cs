using System.Windows;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Admin.Products.BarcodeLabel
{
    public partial class BarcodeLabelDialog : Window
    {
        public BarcodeLabelDialog()
        {
            InitializeComponent();
        }

        public BarcodeLabelDialog(ProductApiService productApi, IPrintService printService)
            : this()
        {
            // Store or use services as needed for barcode labels
        }
    }
}
