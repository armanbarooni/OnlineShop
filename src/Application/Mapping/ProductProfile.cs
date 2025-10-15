using AutoMapper;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.Images, opt => opt.MapFrom(s => s.ProductImages.OrderBy(i => i.DisplayOrder)))
                .ForMember(d => d.Variants, opt => opt.MapFrom(s => s.ProductVariants.OrderBy(v => v.DisplayOrder)))
                .ForMember(d => d.Materials, opt => opt.MapFrom(s => s.ProductMaterials.Select(pm => pm.Material)))
                .ForMember(d => d.Seasons, opt => opt.MapFrom(s => s.ProductSeasons.Select(ps => ps.Season)))
                .ForMember(d => d.ReviewCount, opt => opt.MapFrom(s => s.ProductReviews.Count))
                .ForMember(d => d.AverageRating, opt => opt.MapFrom(s => 
                    s.ProductReviews.Any() ? s.ProductReviews.Average(r => r.Rating) : 0));

            CreateMap<Product, ProductDetailsDto>();

            CreateMap<CreateProductDto, Product>()
                .ForMember(d => d.Id, opt => opt.Ignore())
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

            CreateMap<UpdateProductDto, Product>()
                .ForMember(d => d.Id, opt => opt.Ignore())
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
