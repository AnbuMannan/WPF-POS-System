using System.Globalization;
using System.IO;
using System.Text;
using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public static class EODExportService
    {
        public static void ExportToCsv(EODReportDto report, DateTime reportDate, decimal openingCash, decimal actualCash, decimal expectedCash, decimal cashDifference)
        {
            var sb = new StringBuilder();
            sb.AppendLine("End of Day Report");
            sb.AppendLine($"Date,{reportDate:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("Summary");
            sb.AppendLine($"Total Sales Count,{report.SaleCount}");
            sb.AppendLine($"Total Revenue,{report.TotalSales.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Subtotal,{report.SubtotalSum.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Tax,{report.TaxSum.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Discount,{report.DiscountSum.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine();
            sb.AppendLine("Tax Breakdown");
            sb.AppendLine($"CGST,{report.TotalCGST.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"SGST,{report.TotalSGST.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"IGST,{report.TotalIGST.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine();
            sb.AppendLine("Payment Breakdown");
            foreach (var kv in report.PaymentBreakdown)
                sb.AppendLine($"{kv.Key},{kv.Value.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine();
            sb.AppendLine("Returns");
            sb.AppendLine($"Returns Count,{report.TotalReturnsCount}");
            sb.AppendLine($"Total Refunds,{report.TotalRefunds.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine();
            sb.AppendLine("Cash Reconciliation");
            sb.AppendLine($"Opening Cash,{openingCash.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Cash Sales,{report.CashSalesAmount.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Cash Refunds,{report.CashRefundAmount.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Expected Closing,{expectedCash.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Actual Cash,{actualCash.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Difference,{cashDifference.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine();
            sb.AppendLine("Top Sales");
            sb.AppendLine("BillNumber,TotalAmount,CreatedAt");
            foreach (var s in report.TopSales)
                sb.AppendLine($"{Escape(s.BillNumber)},{s.TotalAmount.ToString("N2", CultureInfo.InvariantCulture)},{s.CreatedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine("Top Selling Products");
            sb.AppendLine("ProductId,Name,SKU,QuantitySold,Revenue");
            foreach (var p in report.TopSellingProducts)
                sb.AppendLine($"{p.ProductId},{Escape(p.Name)},{Escape(p.SKU)},{p.QuantitySold.ToString("N2", CultureInfo.InvariantCulture)},{p.Revenue.ToString("N2", CultureInfo.InvariantCulture)}");

            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            try
            {
                var path = Path.Combine(dir, $"EOD_Report_{reportDate:yyyy-MM-dd}.csv");
                File.WriteAllText(path, sb.ToString());
            }
            catch
            {
                dir = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(dir, $"EOD_Report_{reportDate:yyyy-MM-dd}.csv");
                File.WriteAllText(path, sb.ToString());
            }
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }
    }
}
