using AutoMapper;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.API.Mappings;

/// <summary>
/// AutoMapper profile: Domain Entities &lt;-&gt; Shared DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Category
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));
        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null));
        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Brand, opt => opt.Ignore())
            .ForMember(dest => dest.TaxProfile, opt => opt.Ignore());

        // Brand
        CreateMap<Brand, BrandDto>();
        CreateMap<BrandDto, Brand>()
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        // Supplier
        CreateMap<Supplier, SupplierDto>();
        CreateMap<SupplierDto, Supplier>();

        // PurchaseOrder
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(dest => dest.PurchaseOrderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : null));
        CreateMap<PurchaseOrderDto, PurchaseOrder>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PurchaseOrderId))
            .ForMember(dest => dest.Supplier, opt => opt.Ignore());
        CreateMap<CreatePurchaseOrderDto, PurchaseOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        // PurchaseOrderItem
        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(dest => dest.PurchaseOrderItemId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : null));
        CreateMap<PurchaseOrderItemDto, PurchaseOrderItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PurchaseOrderItemId))
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore());
        CreateMap<CreatePurchaseOrderItemDto, PurchaseOrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrderId, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        // PurchaseEntry
        CreateMap<PurchaseEntry, PurchaseEntryDto>()
            .ForMember(dest => dest.PurchaseEntryId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : null))
            .ForMember(dest => dest.PurchaseOrderReferenceNo, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.ReferenceNo : null));
        CreateMap<PurchaseEntryDto, PurchaseEntry>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PurchaseEntryId))
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore());
        CreateMap<CreatePurchaseEntryDto, PurchaseEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
            .ForMember(dest => dest.IsProcessed, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        // PurchaseEntryItem
        CreateMap<PurchaseEntryItem, PurchaseEntryItemDto>()
            .ForMember(dest => dest.PurchaseEntryItemId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : null));
        CreateMap<PurchaseEntryItemDto, PurchaseEntryItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PurchaseEntryItemId))
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseEntry, opt => opt.Ignore());
        CreateMap<CreatePurchaseEntryItemDto, PurchaseEntryItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseEntryId, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseEntry, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        // Batch
        CreateMap<Batch, BatchDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : null))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.CurrentQuantity - src.AllocatedQuantity))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.ExpiryDate.HasValue && src.ExpiryDate.Value < DateTime.Now));

        // SupplierPayment
        CreateMap<SupplierPayment, SupplierPaymentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : null));
        CreateMap<CreateSupplierPaymentDto, SupplierPayment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentNo, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        // SupplierTransaction
        CreateMap<SupplierTransaction, SupplierTransactionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : null));
    }
}
