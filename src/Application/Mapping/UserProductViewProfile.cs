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
            CreateMap<CreateUserProductViewDto, UserProductView>();
        }
    }
}
