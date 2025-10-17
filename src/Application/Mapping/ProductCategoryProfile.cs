using AutoMapper;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductCategoryProfile : Profile
    {
        public ProductCategoryProfile()
        {
            CreateMap<ProductCategory, ProductCategoryDto>()
                .ForMember(d => d.ParentName, opt => opt.MapFrom(s => s.ParentCategory != null ? s.ParentCategory.Name : null));
            
            CreateMap<ProductCategory, ProductCategoryDetailsDto>();

            CreateMap<CreateProductCategoryDto, ProductCategory>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Level, opt => opt.Ignore())
                .ForMember(d => d.ParentCategory, opt => opt.Ignore())
                .ForMember(d => d.SubCategories, opt => opt.Ignore())
                .ForMember(d => d.MahakId, opt => opt.Ignore())
                .ForMember(d => d.MahakClientId, opt => opt.Ignore())
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ForMember(d => d.Deleted, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
                .ForMember(d => d.LastModifiedAt, opt => opt.Ignore())
                .ForMember(d => d.LastModifiedBy, opt => opt.Ignore());

            CreateMap<UpdateProductCategoryDto, ProductCategory>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.ParentCategoryId, opt => opt.Ignore())
                .ForMember(d => d.Level, opt => opt.Ignore())
                .ForMember(d => d.ParentCategory, opt => opt.Ignore())
                .ForMember(d => d.SubCategories, opt => opt.Ignore())
                .ForMember(d => d.MahakId, opt => opt.Ignore())
                .ForMember(d => d.MahakClientId, opt => opt.Ignore())
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ForMember(d => d.Deleted, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
                .ForMember(d => d.LastModifiedAt, opt => opt.Ignore())
                .ForMember(d => d.LastModifiedBy, opt => opt.Ignore());
        }
    }
}
