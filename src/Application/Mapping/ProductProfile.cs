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
                .ForMember(d => d.CategoryId, opt => opt.Ignore())
                .ForMember(d => d.UnitId, opt => opt.Ignore())
                .ForMember(d => d.BrandId, opt => opt.Ignore())
                .ForMember(d => d.Gender, opt => opt.Ignore())
                .ForMember(d => d.Sku, opt => opt.Ignore())
                .ForMember(d => d.Barcode, opt => opt.Ignore())
                .ForMember(d => d.Weight, opt => opt.Ignore())
                .ForMember(d => d.Dimensions, opt => opt.Ignore())
                .ForMember(d => d.IsActive, opt => opt.Ignore())
                .ForMember(d => d.IsFeatured, opt => opt.Ignore())
                .ForMember(d => d.ViewCount, opt => opt.Ignore())
                .ForMember(d => d.SalePrice, opt => opt.Ignore())
                .ForMember(d => d.SaleStartDate, opt => opt.Ignore())
                .ForMember(d => d.SaleEndDate, opt => opt.Ignore())
                .ForMember(d => d.Category, opt => opt.Ignore())
                .ForMember(d => d.Unit, opt => opt.Ignore())
                .ForMember(d => d.Brand, opt => opt.Ignore())
                .ForMember(d => d.ProductDetails, opt => opt.Ignore())
                .ForMember(d => d.ProductImages, opt => opt.Ignore())
                .ForMember(d => d.ProductReviews, opt => opt.Ignore())
                .ForMember(d => d.ProductInventories, opt => opt.Ignore())
                .ForMember(d => d.ProductVariants, opt => opt.Ignore())
                .ForMember(d => d.ProductMaterials, opt => opt.Ignore())
                .ForMember(d => d.ProductSeasons, opt => opt.Ignore())
                .ForMember(d => d.Wishlists, opt => opt.Ignore())
                .ForMember(d => d.OrderItems, opt => opt.Ignore())
                .ForMember(d => d.CartItems, opt => opt.Ignore())
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
                .ForMember(d => d.CategoryId, opt => opt.Ignore())
                .ForMember(d => d.UnitId, opt => opt.Ignore())
                .ForMember(d => d.BrandId, opt => opt.Ignore())
                .ForMember(d => d.Gender, opt => opt.Ignore())
                .ForMember(d => d.Sku, opt => opt.Ignore())
                .ForMember(d => d.Barcode, opt => opt.Ignore())
                .ForMember(d => d.Weight, opt => opt.Ignore())
                .ForMember(d => d.Dimensions, opt => opt.Ignore())
                .ForMember(d => d.IsActive, opt => opt.Ignore())
                .ForMember(d => d.IsFeatured, opt => opt.Ignore())
                .ForMember(d => d.ViewCount, opt => opt.Ignore())
                .ForMember(d => d.SalePrice, opt => opt.Ignore())
                .ForMember(d => d.SaleStartDate, opt => opt.Ignore())
                .ForMember(d => d.SaleEndDate, opt => opt.Ignore())
                .ForMember(d => d.Category, opt => opt.Ignore())
                .ForMember(d => d.Unit, opt => opt.Ignore())
                .ForMember(d => d.Brand, opt => opt.Ignore())
                .ForMember(d => d.ProductDetails, opt => opt.Ignore())
                .ForMember(d => d.ProductImages, opt => opt.Ignore())
                .ForMember(d => d.ProductReviews, opt => opt.Ignore())
                .ForMember(d => d.ProductInventories, opt => opt.Ignore())
                .ForMember(d => d.ProductVariants, opt => opt.Ignore())
                .ForMember(d => d.ProductMaterials, opt => opt.Ignore())
                .ForMember(d => d.ProductSeasons, opt => opt.Ignore())
                .ForMember(d => d.Wishlists, opt => opt.Ignore())
                .ForMember(d => d.OrderItems, opt => opt.Ignore())
                .ForMember(d => d.CartItems, opt => opt.Ignore())
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
