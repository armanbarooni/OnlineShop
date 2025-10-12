using AutoMapper;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class UserPaymentProfile : Profile
    {
        public UserPaymentProfile()
        {
            CreateMap<UserPayment, UserPaymentDto>();
            CreateMap<CreateUserPaymentDto, UserPayment>();
            CreateMap<UpdateUserPaymentDto, UserPayment>();
        }
    }
}
