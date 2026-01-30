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

        // Customer (entity RowVersion is DateTime; DTO keeps byte[]? for API)
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => src.RowVersion == default ? null : BitConverter.GetBytes(src.RowVersion.Ticks)));
        CreateMap<CustomerDto, Customer>()
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => src.RowVersion == null || src.RowVersion.Length < 8 ? default(DateTime) : new DateTime(BitConverter.ToInt64(src.RowVersion, 0))));

        // Brand
        CreateMap<Brand, BrandDto>();
        CreateMap<BrandDto, Brand>()
            .ForMember(dest => dest.Products, opt => opt.Ignore());
    }
}
