using AutoMapper;
using OnlineShop.Application.DTOs.Brand;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class BrandProfile : Profile
    {
        public BrandProfile()
        {
            CreateMap<Brand, BrandDto>();
            CreateMap<CreateBrandDto, Brand>();
            CreateMap<UpdateBrandDto, Brand>();
        }
    }
}

