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
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();
        }
    }
}
