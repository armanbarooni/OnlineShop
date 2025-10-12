using AutoMapper;
using OnlineShop.Application.DTOs.SavedCart;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class SavedCartProfile : Profile
    {
        public SavedCartProfile()
        {
            CreateMap<SavedCart, SavedCartDto>();
            CreateMap<CreateSavedCartDto, SavedCart>();
            CreateMap<UpdateSavedCartDto, SavedCart>();
        }
    }
}
