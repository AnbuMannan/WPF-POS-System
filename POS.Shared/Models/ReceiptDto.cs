using System;
using System.Collections.Generic;

namespace POS.Shared.Models
{
    /// <summary>
    /// DTO for receipt printing data.
    /// Contains all information needed to print a receipt.
    /// </summary>
    public class ReceiptDto
    {
        public int SaleId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public string BillNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public DateTime SaleDate { get; set; }
        
        // Store Information
        public string StoreName { get; set; } = string.Empty;
        public string? StoreAddress { get; set; }
        public string? StorePhone { get; set; }
        public string? StoreEmail { get; set; }
        public string? GSTIN { get; set; }
        public string? StoreGSTIN { get; set; }
        
        // Customer Information
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        
        // Transaction Details
        public List<ReceiptItemDto> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        /// <summary>Split payments: method name and amount.</summary>
        public List<ReceiptPaymentDto>? Payments { get; set; }
        public decimal? CGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal? IGST { get; set; }
        public decimal? RoundOff { get; set; }
        public string? CashierName { get; set; }
        public int TotalItemCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public int? LoyaltyPointsEarned { get; set; }
        public decimal? YouSaved { get; set; }
        public string? BarcodeBillNumber { get; set; }

        // Footer
        public string? FooterMessage { get; set; }
        public string? ThankYouMessage { get; set; }
    }

    /// <summary>
    /// Individual item on a receipt.
    /// </summary>
    public class ReceiptItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? Discount { get; set; }
        public string? HSNCode { get; set; }
    }

    public class ReceiptPaymentDto
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
    }
}
