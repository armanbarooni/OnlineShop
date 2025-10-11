using AutoMapper;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>();
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
