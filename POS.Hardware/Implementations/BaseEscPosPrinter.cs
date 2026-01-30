using System;
using System.Text;

namespace POS.Hardware.Implementations;

/// <summary>
/// Base class for ESC/POS printer implementations.
/// Provides common ESC/POS command definitions and helper methods.
/// All hardware-specific implementations should inherit from this class.
/// </summary>
public abstract class BaseEscPosPrinter
{
    // ================= ESC/POS COMMAND CONSTANTS =================

    /// <summary>
    /// ESC @ - Initialize printer (clears buffer and resets printer)
    /// </summary>
    protected static readonly byte[] ESC_INIT = { 0x1B, 0x40 };

    /// <summary>
    /// ESC a n - Select justification (0=left, 1=center, 2=right)
    /// </summary>
    protected static byte[] ESC_JUSTIFY_LEFT = { 0x1B, 0x61, 0x00 };
    protected static byte[] ESC_JUSTIFY_CENTER = { 0x1B, 0x61, 0x01 };
    protected static byte[] ESC_JUSTIFY_RIGHT = { 0x1B, 0x61, 0x02 };

    /// <summary>
    /// ESC d n - Print and feed n lines
    /// </summary>
    protected static byte[] ESC_FEED_LINE = { 0x1B, 0x64, 0x01 };

    /// <summary>
    /// ESC E n - Bold font (n=1 on, n=0 off)
    /// </summary>
    protected static byte[] ESC_BOLD_ON = { 0x1B, 0x45, 0x01 };
    protected static byte[] ESC_BOLD_OFF = { 0x1B, 0x45, 0x00 };

    /// <summary>
    /// ESC M n - Select character font (0=Font A, 1=Font B, 2=Font C)
    /// </summary>
    protected static byte[] ESC_FONT_A = { 0x1B, 0x4D, 0x00 };
    protected static byte[] ESC_FONT_B = { 0x1B, 0x4D, 0x01 };

    /// <summary>
    /// GS ! n - Select character size (n = width*16 + height)
    /// </summary>
    protected static byte[] GS_NORMAL_SIZE = { 0x1D, 0x21, 0x00 };
    protected static byte[] GS_DOUBLE_WIDTH = { 0x1D, 0x21, 0x20 };
    protected static byte[] GS_DOUBLE_HEIGHT = { 0x1D, 0x21, 0x10 };
    protected static byte[] GS_DOUBLE_SIZE = { 0x1D, 0x21, 0x30 };

    /// <summary>
    /// GS V m n - Cut paper (m=0 full cut, m=1 partial cut)
    /// </summary>
    protected static byte[] GS_CUT_PAPER_FULL = { 0x1D, 0x56, 0x00 };
    protected static byte[] GS_CUT_PAPER_PARTIAL = { 0x1D, 0x56, 0x01 };

    /// <summary>
    /// ESC p m t1 t2 - Open cash drawer
    /// m=0 or 1 (pin), t1=on time (0-255), t2=off time (0-255)
    /// </summary>
    protected static byte[] ESC_OPEN_DRAWER = { 0x1B, 0x70, 0x00, 0x19, 0xFF };

    /// <summary>
    /// ESC i - Print and cut (auto-cut)
    /// </summary>
    protected static byte[] ESC_PRINT_CUT = { 0x1B, 0x69 };

    /// <summary>
    /// ESC m - Partial cut (feed and cut)
    /// </summary>
    protected static byte[] ESC_PARTIAL_CUT = { 0x1B, 0x6D };

    // ================= HELPER METHODS =================

    /// <summary>
    /// Creates a byte array for feeding n lines.
    /// </summary>
    protected static byte[] CreateFeedLines(int lines)
    {
        if (lines < 0 || lines > 255)
            throw new ArgumentOutOfRangeException(nameof(lines), "Lines must be between 0 and 255");

        return new byte[] { 0x1B, 0x64, (byte)lines };
    }

    /// <summary>
    /// Creates a byte array for setting character size.
    /// </summary>
    /// <param name="width">Width multiplier (1-8)</param>
    /// <param name="height">Height multiplier (1-8)</param>
    protected static byte[] CreateCharacterSize(int width, int height)
    {
        if (width < 1 || width > 8 || height < 1 || height > 8)
            throw new ArgumentOutOfRangeException("Width and height must be between 1 and 8");

        byte n = (byte)((width - 1) * 16 + (height - 1));
        return new byte[] { 0x1D, 0x21, n };
    }

    /// <summary>
    /// Converts a string to ESC/POS compatible byte array with encoding.
    /// </summary>
    protected static byte[] EncodeString(string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.GetEncoding("IBM437"); // Standard ESC/POS encoding
        return encoding.GetBytes(text);
    }

    /// <summary>
    /// Combines multiple byte arrays into one.
    /// </summary>
    protected static byte[] CombineBytes(params byte[][] arrays)
    {
        int totalLength = 0;
        foreach (var array in arrays)
        {
            totalLength += array.Length;
        }

        byte[] result = new byte[totalLength];
        int offset = 0;
        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }

    /// <summary>
    /// Creates a separator line using dashes.
    /// </summary>
    protected static byte[] CreateSeparatorLine(int width = 48)
    {
        string line = new string('-', width);
        return CombineBytes(EncodeString(line), ESC_FEED_LINE);
    }

    /// <summary>
    /// Creates a double separator line using equals signs.
    /// </summary>
    protected static byte[] CreateDoubleSeparatorLine(int width = 48)
    {
        string line = new string('=', width);
        return CombineBytes(EncodeString(line), ESC_FEED_LINE);
    }

    // ================= ABSTRACT METHODS =================

    /// <summary>
    /// Sends raw byte data to the printer.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract Task<bool> SendDataAsync(byte[] data);

    /// <summary>
    /// Initializes the printer connection.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract Task<bool> InitializeAsync();

    /// <summary>
    /// Closes the printer connection and disposes resources.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract Task DisposeAsync(bool disposing);
}
