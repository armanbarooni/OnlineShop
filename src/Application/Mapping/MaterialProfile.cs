using AutoMapper;
using OnlineShop.Application.DTOs.Material;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class MaterialProfile : Profile
    {
        public MaterialProfile()
        {
            CreateMap<Material, MaterialDto>();
            CreateMap<CreateMaterialDto, Material>();
            CreateMap<UpdateMaterialDto, Material>();
        }
    }
}

