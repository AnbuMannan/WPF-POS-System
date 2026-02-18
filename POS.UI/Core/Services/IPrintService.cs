using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public interface IPrintService
    {
        Task<bool> PrintReceiptAsync(ReceiptDto receipt);
        Task<bool> PrintBarcodeLabel(string barcode, string productName, decimal price);

        void PrintEODReport(EODReportDto report, DateTime reportDate, decimal openingCash, decimal actualCash, decimal expectedCash, decimal cashDifference);
        void PrintProfitLossReport(ProfitLossReportDto report);
    }
}
