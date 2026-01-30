using POS.Shared.Models;

namespace POS.Hardware.Interfaces;

/// <summary>
/// Hardware Abstraction Layer interface for printer operations.
/// Provides a standardized way to interact with receipt printers regardless of manufacturer.
/// </summary>
public interface IPrinterService
{
    /// <summary>
    /// Prints a receipt asynchronously.
    /// </summary>
    /// <param name="receipt">The receipt data to print</param>
    /// <returns>True if printing was successful, false otherwise</returns>
    Task<bool> PrintReceiptAsync(ReceiptDto receipt);

    /// <summary>
    /// Gets the current status of the printer.
    /// </summary>
    /// <returns>Printer status information</returns>
    Task<PrinterStatus> GetStatusAsync();

    /// <summary>
    /// Opens the cash drawer connected to the printer.
    /// </summary>
    /// <returns>True if the drawer was opened successfully, false otherwise</returns>
    Task<bool> OpenCashDrawerAsync();
}

/// <summary>
/// Represents the status of a printer.
/// </summary>
public class PrinterStatus
{
    public bool IsOnline { get; set; }
    public bool IsPaperOut { get; set; }
    public bool IsCoverOpen { get; set; }
    public bool IsDrawerOpen { get; set; }
    public string? ErrorMessage { get; set; }
    public string PrinterName { get; set; } = string.Empty;
}
