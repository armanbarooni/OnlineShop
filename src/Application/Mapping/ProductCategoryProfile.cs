using AutoMapper;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductCategoryProfile : Profile
    {
        public ProductCategoryProfile()
        {
            CreateMap<ProductCategory, ProductCategoryDto>();
            CreateMap<ProductCategory, ProductCategoryDetailsDto>();
            CreateMap<CreateProductCategoryDto, ProductCategory>();
            CreateMap<UpdateProductCategoryDto, ProductCategory>();
        }
    }
}
