using AutoMapper;
using OnlineShop.Application.DTOs.UserAddress;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class UserAddressProfile : Profile
    {
        public UserAddressProfile()
        {
            CreateMap<UserAddress, UserAddressDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<CreateUserAddressDto, UserAddress>()
                .ConstructUsing(src => UserAddress.Create(
                    Guid.Empty, // UserId will be set in handler
                    src.Title,
                    src.FirstName,
                    src.LastName,
                    src.AddressLine1,
                    src.City,
                    src.State,
                    src.PostalCode,
                    src.Country
                ));

            CreateMap<UpdateUserAddressDto, UserAddress>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Deleted, opt => opt.Ignore())
                .ForMember(dest => dest.MahakId, opt => opt.Ignore())
                .ForMember(dest => dest.MahakClientId, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
