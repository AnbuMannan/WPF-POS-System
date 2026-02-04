namespace POS.UI.Core.Services
{
    public interface IPrintSettingsService
    {
        void SaveLastPrintedSaleId(int saleId);
        int? GetLastPrintedSaleId();
    }
}
