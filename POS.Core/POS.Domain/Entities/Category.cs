using System;

namespace POS.Domain.Entities
{
    /// <summary>
    /// Category Master Entity (Supports Hierarchy + HSN + Audit)
    /// </summary>
    public class Category
    {
        // ================= PRIMARY =================

        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // ================= HIERARCHY =================

        /// <summary>
        /// Parent Category Id (NULL = Root Category)
        /// </summary>
        public Guid? ParentCategoryId { get; set; }

        // Optional navigation property (if using EF later)
        // public Category? ParentCategory { get; set; }

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
