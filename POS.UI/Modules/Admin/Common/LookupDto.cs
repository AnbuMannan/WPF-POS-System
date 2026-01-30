namespace POS.UI.Modules.Admin.Common;

/// <summary>
/// Lookup item for ComboBox (Category, Brand, TaxProfile, etc.).
/// Id is long so it can hold int (CategoryId, BrandId, TaxProfileId) or long (ProductId).
/// Use 0 for "no selection" / "no parent" where applicable.
/// </summary>
public class LookupDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
