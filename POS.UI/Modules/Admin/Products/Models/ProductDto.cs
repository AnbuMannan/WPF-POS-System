using System;

namespace POS.UI.Modules.Admin.Products.Models
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }

        public Guid CategoryId { get; set; }
        public Guid BrandId { get; set; }

        public string Unit { get; set; }

        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MRP { get; set; }

        public string HSNCode { get; set; }
        public Guid TaxProfileId { get; set; }

        public bool IsWeighable { get; set; }
        public bool IsManufactured { get; set; }
        public bool IsTaxInclusive { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}