namespace POS.UI.Core.Services
{
    public class PrintSettingsService : IPrintSettingsService
    {
        private int? _lastPrintedSaleId;

        public void SaveLastPrintedSaleId(int saleId)
        {
            _lastPrintedSaleId = saleId;
        }

        public int? GetLastPrintedSaleId()
        {
            return _lastPrintedSaleId;
        }
    }
}
