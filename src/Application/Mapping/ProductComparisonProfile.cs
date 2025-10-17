using AutoMapper;
using OnlineShop.Application.DTOs.ProductComparison;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class ProductComparisonProfile : Profile
    {
        public ProductComparisonProfile()
        {
            CreateMap<ProductComparison, ProductComparisonDto>()
                .ForMember(d => d.ProductCount, opt => opt.MapFrom(s => s.GetProductCount()))
                .ForMember(d => d.IsFull, opt => opt.MapFrom(s => s.IsFull()));
        }
    }
}

