using POS.Shared.Models;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace POS.UI.Core.Services
{
    public class PrintService : IPrintService
    {
        public Task<bool> PrintReceiptAsync(ReceiptDto receipt)
        {
            try
            {
                var dialog = new System.Windows.Controls.PrintDialog();
                var document = CreateReceiptDocument(receipt);
                
                dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"Receipt-{receipt.BillNumber}");
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> PrintBarcodeLabel(string barcode, string productName, decimal price)
        {
            return Task.FromResult(true); // Placeholder
        }

        public void PrintEODReport(EODReportDto report, DateTime reportDate, decimal openingCash, decimal actualCash, decimal expectedCash, decimal cashDifference)
        {
            try
            {
                var dialog = new System.Windows.Controls.PrintDialog();
                var document = CreateEODDocument(report, reportDate, openingCash, actualCash, expectedCash, cashDifference);
                dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"EOD-Report-{reportDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Print EOD report failed.", ex);
            }
        }

        private FlowDocument CreateEODDocument(EODReportDto report, DateTime reportDate, decimal openingCash, decimal actualCash, decimal expectedCash, decimal cashDifference)
        {
            // A4 at 96 DPI: 794 x 1122
            var doc = new FlowDocument
            {
                PageWidth = 794,
                PageHeight = 1122,
                PagePadding = new Thickness(48),
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11
            };

            doc.Blocks.Add(new Paragraph(new Run("END OF DAY REPORT")) { FontSize = 18, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            doc.Blocks.Add(new Paragraph(new Run(reportDate.ToString("dd-MMM-yyyy"))) { FontSize = 14, TextAlignment = TextAlignment.Center });
            doc.Blocks.Add(new Paragraph());

            doc.Blocks.Add(new Paragraph(new Run("Summary")) { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(new Paragraph(new Run($"Total Sales (count): {report.SaleCount}")));
            doc.Blocks.Add(new Paragraph(new Run($"Total Revenue: ₹ {report.TotalSales:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Subtotal: ₹ {report.SubtotalSum:N2}  |  Tax: ₹ {report.TaxSum:N2}  |  Discount: ₹ {report.DiscountSum:N2}")));
            doc.Blocks.Add(new Paragraph());

            doc.Blocks.Add(new Paragraph(new Run("Tax Collected")) { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(new Paragraph(new Run($"CGST: ₹ {report.TotalCGST:N2}  |  SGST: ₹ {report.TotalSGST:N2}  |  IGST: ₹ {report.TotalIGST:N2}")));
            doc.Blocks.Add(new Paragraph());

            doc.Blocks.Add(new Paragraph(new Run("Payment Breakdown")) { FontWeight = FontWeights.Bold });
            foreach (var kv in report.PaymentBreakdown)
                doc.Blocks.Add(new Paragraph(new Run($"{kv.Key}: ₹ {kv.Value:N2}")));
            doc.Blocks.Add(new Paragraph());

            doc.Blocks.Add(new Paragraph(new Run("Returns & Refunds")) { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(new Paragraph(new Run($"Returns count: {report.TotalReturnsCount}  |  Total refunds: ₹ {report.TotalRefunds:N2}")));
            doc.Blocks.Add(new Paragraph());

            doc.Blocks.Add(new Paragraph(new Run("Cash Reconciliation")) { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(new Paragraph(new Run($"Opening cash: ₹ {openingCash:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Cash sales: ₹ {report.CashSalesAmount:N2}  |  Cash refunds: ₹ {report.CashRefundAmount:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Expected closing cash: ₹ {expectedCash:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Actual cash: ₹ {actualCash:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Difference: ₹ {cashDifference:N2}") { Foreground = cashDifference < 0 ? System.Windows.Media.Brushes.DarkRed : System.Windows.Media.Brushes.DarkGreen }));
            doc.Blocks.Add(new Paragraph());

            doc.Blocks.Add(new Paragraph(new Run($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}")) { FontSize = 9 });

            return doc;
        }

        private FlowDocument CreateReceiptDocument(ReceiptDto receipt)
        {
            var doc = new FlowDocument
            {
                PageWidth = 300,
                PageHeight = double.NaN,
                PagePadding = new Thickness(20),
                FontFamily = new System.Windows.Media.FontFamily("Courier New"),
                FontSize = 10
            };

            // Add receipt content
            doc.Blocks.Add(new Paragraph(new Run($"Bill: {receipt.BillNumber}")) { TextAlignment = TextAlignment.Center });
            doc.Blocks.Add(new Paragraph(new Run($"Date: {receipt.SaleDate:dd/MM/yyyy HH:mm}")) { TextAlignment = TextAlignment.Center });
            
            return doc;
        }
    }
}
