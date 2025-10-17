using AutoMapper;
using OnlineShop.Application.DTOs.UserProductView;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class UserProductViewProfile : Profile
    {
        public UserProductViewProfile()
        {
            CreateMap<UserProductView, UserProductViewDto>();
            
            CreateMap<CreateUserProductViewDto, UserProductView>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.ViewedAt, opt => opt.Ignore())
                .ForMember(d => d.ViewDuration, opt => opt.Ignore())
                .ForMember(d => d.IsReturningView, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.Product, opt => opt.Ignore())
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
}
