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
            
            CreateMap<CreateUserPaymentDto, UserPayment>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.PaymentStatus, opt => opt.Ignore())
                .ForMember(d => d.TransactionId, opt => opt.Ignore())
                .ForMember(d => d.GatewayResponse, opt => opt.Ignore())
                .ForMember(d => d.PaidAt, opt => opt.Ignore())
                .ForMember(d => d.FailedAt, opt => opt.Ignore())
                .ForMember(d => d.FailureReason, opt => opt.Ignore())
                .ForMember(d => d.RefundId, opt => opt.Ignore())
                .ForMember(d => d.RefundAmount, opt => opt.Ignore())
                .ForMember(d => d.RefundedAt, opt => opt.Ignore())
                .ForMember(d => d.RefundReason, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.Order, opt => opt.Ignore())
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
            
            CreateMap<UpdateUserPaymentDto, UserPayment>()
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.OrderId, opt => opt.Ignore())
                .ForMember(d => d.PaymentStatus, opt => opt.Ignore())
                .ForMember(d => d.TransactionId, opt => opt.Ignore())
                .ForMember(d => d.GatewayResponse, opt => opt.Ignore())
                .ForMember(d => d.PaidAt, opt => opt.Ignore())
                .ForMember(d => d.FailedAt, opt => opt.Ignore())
                .ForMember(d => d.FailureReason, opt => opt.Ignore())
                .ForMember(d => d.RefundId, opt => opt.Ignore())
                .ForMember(d => d.RefundAmount, opt => opt.Ignore())
                .ForMember(d => d.RefundedAt, opt => opt.Ignore())
                .ForMember(d => d.RefundReason, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.Order, opt => opt.Ignore())
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
