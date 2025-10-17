using AutoMapper;
using OnlineShop.Application.DTOs.ProductImage;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductImageProfile : Profile
    {
        public ProductImageProfile()
        {
            CreateMap<ProductImage, ProductImageDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<CreateProductImageDto, ProductImage>()
                .ConstructUsing(src => ProductImage.Create(
                    src.ProductId,
                    src.ImageUrl,
                    src.AltText,
                    src.Title,
                    src.DisplayOrder,
                    src.IsPrimary,
                    "Main", // Default image type
                    src.FileSize,
                    src.MimeType
                ))
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.ProductId, opt => opt.Ignore())
                .ForMember(d => d.ImageUrl, opt => opt.Ignore())
                .ForMember(d => d.AltText, opt => opt.Ignore())
                .ForMember(d => d.Title, opt => opt.Ignore())
                .ForMember(d => d.DisplayOrder, opt => opt.Ignore())
                .ForMember(d => d.IsPrimary, opt => opt.Ignore())
                .ForMember(d => d.ImageType, opt => opt.Ignore())
                .ForMember(d => d.FileSize, opt => opt.Ignore())
                .ForMember(d => d.MimeType, opt => opt.Ignore())
                .ForMember(d => d.Product, opt => opt.Ignore())
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

            CreateMap<UpdateProductImageDto, ProductImage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.ImageType, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.MahakId, opt => opt.Ignore())
                .ForMember(dest => dest.MahakClientId, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.Deleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore());
        }
    }
}
