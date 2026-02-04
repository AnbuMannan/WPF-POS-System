using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public class EmailReceiptService : IEmailReceiptService
    {
        public Task<bool> SendReceiptEmailAsync(ReceiptDto receipt, string emailAddress)
        {
            // Placeholder implementation
            // In production, this would use SMTP or email API
            return Task.FromResult(true);
        }
    }
}
