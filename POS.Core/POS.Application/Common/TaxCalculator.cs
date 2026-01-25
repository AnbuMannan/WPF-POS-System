namespace POS.Application.Common;

public static class TaxCalculator
{
    public static decimal CalculateTax(decimal price, decimal taxPercent, bool isInclusive)
    {
        if (taxPercent <= 0) return 0;

        if (!isInclusive)
        {
            return Math.Round(price * taxPercent / 100, 2);
        }

        return Math.Round(price * taxPercent / (100 + taxPercent), 2);
    }

    public static (decimal cgst, decimal sgst, decimal igst)
        SplitGST(decimal taxAmount, bool isInterState)
    {
        if (isInterState)
            return (0, 0, taxAmount);

        var half = Math.Round(taxAmount / 2, 2);
        return (half, half, 0);
    }

    public static decimal CalculateBasePrice(decimal price, decimal taxPercent, bool isInclusive)
    {
        if (!isInclusive) return price;

        var tax = CalculateTax(price, taxPercent, true);
        return Math.Round(price - tax, 2);
    }
}
