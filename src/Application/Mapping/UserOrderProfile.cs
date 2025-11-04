using AutoMapper;
using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class UserOrderProfile : Profile
    {
        public UserOrderProfile()
        {
            CreateMap<UserOrder, UserOrderDto>()
                .ForMember(d => d.ShippingCost, opt => opt.Ignore())
                .ForMember(d => d.FinalAmount, opt => opt.Ignore())
                .ForMember(d => d.PaymentStatus, opt => opt.Ignore())
                .ForMember(d => d.OrderDate, opt => opt.Ignore())
                .ForMember(d => d.ShippedDate, opt => opt.Ignore())
                .ForMember(d => d.DeliveredDate, opt => opt.Ignore());
            
            CreateMap<CreateUserOrderDto, UserOrder>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.OrderStatus, opt => opt.Ignore())
                .ForMember(d => d.ShippedAt, opt => opt.Ignore())
                .ForMember(d => d.DeliveredAt, opt => opt.Ignore())
                .ForMember(d => d.CancelledAt, opt => opt.Ignore())
                .ForMember(d => d.CancellationReason, opt => opt.Ignore())
                .ForMember(d => d.TrackingNumber, opt => opt.Ignore())
                .ForMember(d => d.EstimatedDeliveryDate, opt => opt.Ignore())
                .ForMember(d => d.ActualDeliveryDate, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.ShippingAddress, opt => opt.Ignore())
                .ForMember(d => d.BillingAddress, opt => opt.Ignore())
                .ForMember(d => d.OrderItems, opt => opt.Ignore())
                .ForMember(d => d.Payments, opt => opt.Ignore())
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
            
            CreateMap<UpdateUserOrderDto, UserOrder>()
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.OrderNumber, opt => opt.Ignore())
                .ForMember(d => d.Currency, opt => opt.Ignore())
                .ForMember(d => d.ShippedAt, opt => opt.Ignore())
                .ForMember(d => d.DeliveredAt, opt => opt.Ignore())
                .ForMember(d => d.CancelledAt, opt => opt.Ignore())
                .ForMember(d => d.CancellationReason, opt => opt.Ignore())
                .ForMember(d => d.EstimatedDeliveryDate, opt => opt.Ignore())
                .ForMember(d => d.ActualDeliveryDate, opt => opt.Ignore())
                .ForMember(d => d.ShippingAddressId, opt => opt.Ignore())
                .ForMember(d => d.BillingAddressId, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.ShippingAddress, opt => opt.Ignore())
                .ForMember(d => d.BillingAddress, opt => opt.Ignore())
                .ForMember(d => d.OrderItems, opt => opt.Ignore())
                .ForMember(d => d.Payments, opt => opt.Ignore())
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
            
            CreateMap<UserOrderItem, UserOrderItemDto>();
        }
    }
}
