using System;

namespace POS.UI.Core.Models
{
    /// <summary>
    /// UI DTO for Category Master (Hierarchy + HSN + Audit ready)
    /// This model is used only in WPF UI layer.
    /// </summary>
    public class CategoryDto
    {
        // ================= PRIMARY =================

        public Guid CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        // ================= HIERARCHY =================

        /// <summary>
        /// Parent Category Id (NULL = Root Category)
        /// </summary>
        public Guid? ParentCategoryId { get; set; }

        /// <summary>
        /// Parent Category display name (for flat grid)
        /// </summary>
        public string? ParentCategoryName { get; set; }

        /// <summary>
        /// Hierarchy level (0 = root, 1 = child, 2 = sub child…)
        /// Used only for UI indentation
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Indented name for ComboBox display
        /// Example: \"   └─ Rice\"
        /// </summary>
        public string IndentedName
            => new string(' ', Level * 3) + Name;

        // ================= HSN =================

        /// <summary>
        /// Optional HSN mapped at category / subcategory level
        /// </summary>
        public string? HSNCode { get; set; }

        // ================= ORDERING =================

        public int DisplayOrder { get; set; }

        // ================= STATUS =================

        public bool IsActive { get; set; }

        // ================= AUDIT =================

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
