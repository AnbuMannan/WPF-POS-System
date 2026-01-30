namespace POS.Shared.Models;

/// <summary>
/// Shared DTO for Category Master (Hierarchy + HSN + Audit).
/// Used across UI and API layers.
/// </summary>
public class CategoryDto
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Slug { get; set; }

    /// <summary>Parent Category Id (NULL = Root).</summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>Parent Category display name (for flat grid).</summary>
    public string? ParentCategoryName { get; set; }

    public int Level { get; set; }

    public string? HSNCode { get; set; }
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Indented name for ComboBox, e.g. "   └─ Rice".</summary>
    public string IndentedName => new string(' ', Level * 3) + Name;
}
