using AutoMapper;
using OnlineShop.Application.DTOs.UserProfile;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class UserProfileProfile : Profile
    {
        public UserProfileProfile()
        {
            CreateMap<UserProfile, UserProfileDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<CreateUserProfileDto, UserProfile>()
                .ConstructUsing(src => UserProfile.Create(
                    src.UserId,
                    src.FirstName,
                    src.LastName
                ))
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.FirstName, opt => opt.Ignore())
                .ForMember(d => d.LastName, opt => opt.Ignore())
                .ForMember(d => d.NationalCode, opt => opt.Ignore())
                .ForMember(d => d.BirthDate, opt => opt.Ignore())
                .ForMember(d => d.Gender, opt => opt.Ignore())
                .ForMember(d => d.ProfileImageUrl, opt => opt.Ignore())
                .ForMember(d => d.Bio, opt => opt.Ignore())
                .ForMember(d => d.Website, opt => opt.Ignore())
                .ForMember(d => d.IsEmailVerified, opt => opt.Ignore())
                .ForMember(d => d.IsPhoneVerified, opt => opt.Ignore())
                .ForMember(d => d.EmailVerifiedAt, opt => opt.Ignore())
                .ForMember(d => d.PhoneVerifiedAt, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
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

            CreateMap<UpdateUserProfileDto, UserProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.IsPhoneVerified, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneVerifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.MahakId, opt => opt.Ignore())
                .ForMember(dest => dest.MahakClientId, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.Deleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore());
        }
    }
}
