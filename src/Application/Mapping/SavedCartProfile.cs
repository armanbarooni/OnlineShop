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
            
            CreateMap<CreateSavedCartDto, SavedCart>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.SavedAt, opt => opt.Ignore())
                .ForMember(d => d.LastAccessedAt, opt => opt.Ignore())
                .ForMember(d => d.AccessCount, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.Cart, opt => opt.Ignore())
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
            
            CreateMap<UpdateSavedCartDto, SavedCart>()
                .ForMember(d => d.CartId, opt => opt.Ignore())
                .ForMember(d => d.SavedAt, opt => opt.Ignore())
                .ForMember(d => d.LastAccessedAt, opt => opt.Ignore())
                .ForMember(d => d.AccessCount, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.Cart, opt => opt.Ignore())
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
