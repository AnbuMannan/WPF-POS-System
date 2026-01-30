using System.Diagnostics;
using POS.Hardware.Interfaces;
using POS.Shared.Models;

namespace POS.Hardware.Implementations;

/// <summary>
/// Mock implementation of IPrinterService for development and testing.
/// Simulates printer operations using Debug.WriteLine without requiring physical hardware.
/// </summary>
public class MockPrinterService : IPrinterService
{
    private readonly string _printerName;

    public MockPrinterService(string printerName = "Mock Printer")
    {
        _printerName = printerName;
    }

    public async Task<bool> PrintReceiptAsync(ReceiptDto receipt)
    {
        try
        {
            // Simulate async operation
            await Task.Delay(100);

            Debug.WriteLine("========================================");
            Debug.WriteLine("MOCK PRINTER: Printing Receipt");
            Debug.WriteLine("========================================");
            Debug.WriteLine($"Receipt Number: {receipt.ReceiptNumber}");
            Debug.WriteLine($"Date: {receipt.TransactionDate:yyyy-MM-dd HH:mm:ss}");
            Debug.WriteLine("");

            // Store Information
            if (!string.IsNullOrWhiteSpace(receipt.StoreName))
            {
                Debug.WriteLine($"Store: {receipt.StoreName}");
            }
            if (!string.IsNullOrWhiteSpace(receipt.StoreAddress))
            {
                Debug.WriteLine($"Address: {receipt.StoreAddress}");
            }
            if (!string.IsNullOrWhiteSpace(receipt.StorePhone))
            {
                Debug.WriteLine($"Phone: {receipt.StorePhone}");
            }
            if (!string.IsNullOrWhiteSpace(receipt.GSTIN))
            {
                Debug.WriteLine($"GSTIN: {receipt.GSTIN}");
            }
            Debug.WriteLine("");

            // Customer Information
            if (!string.IsNullOrWhiteSpace(receipt.CustomerName))
            {
                Debug.WriteLine($"Customer: {receipt.CustomerName}");
                if (!string.IsNullOrWhiteSpace(receipt.CustomerPhone))
                {
                    Debug.WriteLine($"Phone: {receipt.CustomerPhone}");
                }
                Debug.WriteLine("");
            }

            // Items
            Debug.WriteLine("Items:");
            Debug.WriteLine("----------------------------------------");
            foreach (var item in receipt.Items)
            {
                Debug.WriteLine($"{item.ProductName}");
                Debug.WriteLine($"  {item.Quantity} {item.Unit} x {item.UnitPrice:C} = {item.TotalPrice:C}");
                if (item.Discount.HasValue && item.Discount.Value > 0)
                {
                    Debug.WriteLine($"  Discount: {item.Discount.Value:C}");
                }
            }
            Debug.WriteLine("----------------------------------------");

            // Totals
            Debug.WriteLine($"Subtotal: {receipt.SubTotal:C}");
            if (receipt.Discount > 0)
            {
                Debug.WriteLine($"Discount: -{receipt.Discount:C}");
            }
            if (receipt.TaxAmount > 0)
            {
                Debug.WriteLine($"Tax: {receipt.TaxAmount:C}");
            }
            Debug.WriteLine($"Total: {receipt.TotalAmount:C}");
            Debug.WriteLine($"Paid: {receipt.AmountPaid:C}");
            if (receipt.ChangeAmount > 0)
            {
                Debug.WriteLine($"Change: {receipt.ChangeAmount:C}");
            }
            Debug.WriteLine($"Payment: {receipt.PaymentMethod}");
            Debug.WriteLine("");

            // Footer
            if (!string.IsNullOrWhiteSpace(receipt.FooterMessage))
            {
                Debug.WriteLine(receipt.FooterMessage);
            }
            if (!string.IsNullOrWhiteSpace(receipt.ThankYouMessage))
            {
                Debug.WriteLine(receipt.ThankYouMessage);
            }

            Debug.WriteLine("========================================");
            Debug.WriteLine("MOCK PRINTER: Receipt printed successfully");
            Debug.WriteLine("========================================");

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MOCK PRINTER ERROR: {ex.Message}");
            return false;
        }
    }

    public async Task<PrinterStatus> GetStatusAsync()
    {
        try
        {
            await Task.Delay(50); // Simulate async operation

            return new PrinterStatus
            {
                IsOnline = true,
                IsPaperOut = false,
                IsCoverOpen = false,
                IsDrawerOpen = false,
                ErrorMessage = null,
                PrinterName = _printerName
            };
        }
        catch (Exception ex)
        {
            return new PrinterStatus
            {
                IsOnline = false,
                ErrorMessage = ex.Message,
                PrinterName = _printerName
            };
        }
    }

    public async Task<bool> OpenCashDrawerAsync()
    {
        try
        {
            await Task.Delay(100); // Simulate async operation

            Debug.WriteLine("MOCK PRINTER: Cash drawer opened (simulated)");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MOCK PRINTER ERROR: Failed to open cash drawer - {ex.Message}");
            return false;
        }
    }
}
