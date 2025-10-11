using AutoMapper;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlineShop.Application.Mapping;

public class UnitProfile : Profile
{
    public UnitProfile()
    {
        CreateMap<Unit, UnitDto>();
        CreateMap<Unit, UnitDetailsDto>();

        CreateMap<CreateUnitDto, Unit>()
            .ForMember(d => d.UnitCode, opt => opt.Ignore())
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

        CreateMap<UpdateUnitDto, Unit>()
            .ForMember(d => d.UnitCode, opt => opt.Ignore())
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
