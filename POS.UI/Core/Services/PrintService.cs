using POS.Shared.Models;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Serilog;

// Explicitly use WPF types to avoid conflicts with System.Drawing/Windows.Forms
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Orientation = System.Windows.Controls.Orientation;
using Size = System.Windows.Size;

namespace POS.UI.Core.Services
{
    public class PrintService : IPrintService
    {
        private readonly ILogger _logger = Log.ForContext<PrintService>();
        private readonly string _labelsFolder;

        public PrintService()
        {
            // Create Labels folder in application directory
            _labelsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Labels");
            if (!Directory.Exists(_labelsFolder))
            {
                Directory.CreateDirectory(_labelsFolder);
            }
        }

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
            try
            {
                // Create a unique filename with timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var safeProductName = string.Join("_", productName.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"Label_{safeProductName}_{timestamp}.xps";
                var filePath = Path.Combine(_labelsFolder, fileName);

                // Create the label visual
                var labelVisual = CreateBarcodeLabelVisual(barcode, productName, price);

                // Save as XPS (which can be converted to PDF or printed directly)
                SaveVisualToXps(labelVisual, filePath);

                _logger.Information("Label saved to: {FilePath}", filePath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save barcode label for {ProductName}", productName);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Creates a visual representation of a barcode label
        /// Standard label size: 50mm x 25mm (approximately 189 x 94 pixels at 96 DPI)
        /// </summary>
        private FrameworkElement CreateBarcodeLabelVisual(string barcode, string productName, decimal price)
        {
            // Label dimensions (50mm x 25mm at 96 DPI)
            const double labelWidth = 189;
            const double labelHeight = 94;

            var grid = new Grid
            {
                Width = labelWidth,
                Height = labelHeight,
                Background = Brushes.White
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.2, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.8, GridUnitType.Star) });

            // Product Name (top)
            var nameText = new TextBlock
            {
                Text = TruncateText(productName, 25),
                FontSize = 8,
                FontWeight = FontWeights.SemiBold,
                FontFamily = new FontFamily("Arial"),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(4, 2, 4, 0)
            };
            Grid.SetRow(nameText, 0);
            grid.Children.Add(nameText);

            // Barcode representation (middle) - simplified text-based barcode
            var barcodePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Create barcode bars visual (simplified)
            var barsContainer = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(2, 1, 2, 1),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var barsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Generate simple barcode pattern based on barcode string
            foreach (char c in barcode)
            {
                int charValue = (int)c;
                barsPanel.Children.Add(new Border
                {
                    Width = (charValue % 2 == 0) ? 1 : 2,
                    Height = 25,
                    Background = Brushes.Black,
                    Margin = new Thickness(0.5, 0, 0.5, 0)
                });
                barsPanel.Children.Add(new Border
                {
                    Width = (charValue % 3 == 0) ? 2 : 1,
                    Height = 25,
                    Background = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 0)
                });
            }

            barsContainer.Child = barsPanel;
            barcodePanel.Children.Add(barsContainer);

            // Barcode number below bars
            var barcodeText = new TextBlock
            {
                Text = barcode,
                FontSize = 7,
                FontFamily = new FontFamily("Consolas"),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0)
            };
            barcodePanel.Children.Add(barcodeText);

            Grid.SetRow(barcodePanel, 1);
            grid.Children.Add(barcodePanel);

            // Price (bottom)
            var priceText = new TextBlock
            {
                Text = $"₹ {price:N2}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Arial"),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(priceText, 2);
            grid.Children.Add(priceText);

            // Add border around the label
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0.5),
                Child = grid
            };

            // Force measure and arrange
            border.Measure(new Size(labelWidth, labelHeight));
            border.Arrange(new Rect(0, 0, labelWidth, labelHeight));
            border.UpdateLayout();

            return border;
        }

        private void SaveVisualToXps(FrameworkElement visual, string filePath)
        {
            // Ensure the visual is measured and arranged
            visual.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            visual.Arrange(new Rect(visual.DesiredSize));
            visual.UpdateLayout();

            // Create XPS document
            using (var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite))
            {
                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                writer.Write(visual);
            }
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 2) + "..";
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

        public void PrintProfitLossReport(ProfitLossReportDto report)
        {
            try
            {
                var dialog = new System.Windows.Controls.PrintDialog();
                var document = CreateProfitLossDocument(report);
                dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"ProfitLoss-{report.From:yyyy-MM-dd}_{report.To:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Print Profit & Loss report failed.", ex);
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

        private FlowDocument CreateProfitLossDocument(ProfitLossReportDto report)
        {
            var doc = new FlowDocument
            {
                PageWidth = 794,
                PageHeight = 1122,
                PagePadding = new Thickness(48),
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11
            };

            doc.Blocks.Add(new Paragraph(new Run("PROFIT & LOSS REPORT")) { FontSize = 18, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            doc.Blocks.Add(new Paragraph(new Run($"{report.From:dd-MMM-yyyy} to {report.To:dd-MMM-yyyy}")) { FontSize = 14, TextAlignment = TextAlignment.Center });
            doc.Blocks.Add(new Paragraph());

            doc.Blocks.Add(new Paragraph(new Run("Summary")) { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(new Paragraph(new Run($"Total Sales: ₹ {report.TotalSales:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Cost of Goods Sold: ₹ {report.TotalCogs:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Total Expenses: ₹ {report.TotalExpenses:N2}")));

            var netText = report.ProfitLoss >= 0
                ? $"Net Profit: ₹ {report.ProfitLoss:N2}"
                : $"Net Loss: ₹ {-report.ProfitLoss:N2}";

            var netRun = new Run(netText);
            netRun.Foreground = report.ProfitLoss >= 0 ? System.Windows.Media.Brushes.DarkGreen : System.Windows.Media.Brushes.DarkRed;
            doc.Blocks.Add(new Paragraph(netRun));
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
