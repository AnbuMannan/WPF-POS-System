using System.Windows;
using POS.Shared.Models;
using POS.UI.Core.Services;
using MessageBox = System.Windows.MessageBox;

namespace POS.UI.Modules.Billing.ReceiptPreview
{
    public partial class PrintPreviewDialog : Window
    {
        private readonly IPrintService? _printService;
        private readonly IEmailReceiptService? _emailService;
        public ReceiptDto Receipt { get; }

        public PrintPreviewDialog(ReceiptDto receipt, IPrintService? printService, IEmailReceiptService? emailService)
        {
            InitializeComponent();
            Receipt = receipt;
            _printService = printService;
            _emailService = emailService;
            DataContext = new { Receipt = receipt };
        }

        private async void Print_Click(object sender, RoutedEventArgs e)
        {
            if (_printService != null)
            {
                var success = await _printService.PrintReceiptAsync(Receipt);
                if (success)
                {
                    MessageBox.Show("Receipt printed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to print receipt", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
