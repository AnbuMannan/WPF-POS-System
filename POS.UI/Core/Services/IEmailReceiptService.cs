using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public interface IEmailReceiptService
    {
        Task<bool> SendReceiptEmailAsync(ReceiptDto receipt, string emailAddress);
    }
}
