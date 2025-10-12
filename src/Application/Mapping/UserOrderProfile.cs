using AutoMapper;
using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class UserOrderProfile : Profile
    {
        public UserOrderProfile()
        {
            CreateMap<UserOrder, UserOrderDto>();
            CreateMap<CreateUserOrderDto, UserOrder>();
            CreateMap<UpdateUserOrderDto, UserOrder>();
        }
    }
}
