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

            CreateMap<CreateUserReturnRequestDto, UserReturnRequest>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.ReturnStatus, opt => opt.Ignore())
                .ForMember(d => d.AdminNotes, opt => opt.Ignore())
                .ForMember(d => d.ApprovedAt, opt => opt.Ignore())
                .ForMember(d => d.ApprovedBy, opt => opt.Ignore())
                .ForMember(d => d.RejectedAt, opt => opt.Ignore())
                .ForMember(d => d.RejectedBy, opt => opt.Ignore())
                .ForMember(d => d.RejectionReason, opt => opt.Ignore())
                .ForMember(d => d.ProcessedAt, opt => opt.Ignore())
                .ForMember(d => d.ProcessedBy, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.Order, opt => opt.Ignore())
                .ForMember(d => d.OrderItem, opt => opt.Ignore())
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

            CreateMap<UpdateUserReturnRequestDto, UserReturnRequest>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.OrderId, opt => opt.Ignore())
                .ForMember(d => d.OrderItemId, opt => opt.Ignore())
                .ForMember(d => d.ReturnStatus, opt => opt.Ignore())
                .ForMember(d => d.AdminNotes, opt => opt.Ignore())
                .ForMember(d => d.ApprovedAt, opt => opt.Ignore())
                .ForMember(d => d.ApprovedBy, opt => opt.Ignore())
                .ForMember(d => d.RejectedAt, opt => opt.Ignore())
                .ForMember(d => d.RejectedBy, opt => opt.Ignore())
                .ForMember(d => d.RejectionReason, opt => opt.Ignore())
                .ForMember(d => d.ProcessedAt, opt => opt.Ignore())
                .ForMember(d => d.ProcessedBy, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.Order, opt => opt.Ignore())
                .ForMember(d => d.OrderItem, opt => opt.Ignore())
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
