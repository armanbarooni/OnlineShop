using AutoMapper;
using OnlineShop.Application.DTOs.StockAlert;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class StockAlertProfile : Profile
    {
        public StockAlertProfile()
        {
            CreateMap<StockAlert, StockAlertDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductVariantName, opt => opt.MapFrom(src => src.ProductVariant != null ? $"{src.ProductVariant.Size} - {src.ProductVariant.Color}" : null))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ReverseMap();
        }
    }
}
