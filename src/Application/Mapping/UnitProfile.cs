using AutoMapper;
using OnlineShop.Application.DTOs.Unit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlineShop.Application.Mapping;

public class UnitProfile : Profile
{
    public UnitProfile()
    {
        CreateMap<Unit, UnitDto>();
        CreateMap<Unit, UnitDetailsDto>();
        CreateMap<CreateUnitDto, Unit>();
        CreateMap<UpdateUnitDto, Unit>();
    }
}
