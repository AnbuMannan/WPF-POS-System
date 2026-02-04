using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public interface IPrintService
    {
        Task<bool> PrintReceiptAsync(ReceiptDto receipt);
        Task<bool> PrintBarcodeLabel(string barcode, string productName, decimal price);

        /// <summary>Print EOD report (A4 format with summary data).</summary>
        void PrintEODReport(EODReportDto report, DateTime reportDate, decimal openingCash, decimal actualCash, decimal expectedCash, decimal cashDifference);
    }
}
