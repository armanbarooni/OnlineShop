using AutoMapper;
using OnlineShop.Application.DTOs.ProductRelation;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductRelationProfile : Profile
    {
        public ProductRelationProfile()
        {
            CreateMap<ProductRelation, ProductRelationDto>();
            CreateMap<CreateProductRelationDto, ProductRelation>();
            CreateMap<UpdateProductRelationDto, ProductRelation>();
        }
    }
}
