using AutoMapper;
using OnlineShop.Application.DTOs.UserReturnRequest;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class UserReturnRequestProfile : Profile
    {
        public UserReturnRequestProfile()
        {
            CreateMap<UserReturnRequest, UserReturnRequestDto>();
            CreateMap<CreateUserReturnRequestDto, UserReturnRequest>();
            CreateMap<UpdateUserReturnRequestDto, UserReturnRequest>();
        }
    }
}
