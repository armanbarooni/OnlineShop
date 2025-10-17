using AutoMapper;
using OnlineShop.Application.DTOs.OrderStatusHistory;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;

namespace OnlineShop.Application.Mapping
{
    public class OrderStatusHistoryProfile : Profile
    {
        public OrderStatusHistoryProfile()
        {
            CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
                .ForMember(dest => dest.StatusDisplayName, opt => opt.MapFrom(src => GetStatusDisplayName(src.Status)));

            CreateMap<OrderStatusHistory, OrderTimelineDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentStatus, opt => opt.Ignore())
                .ForMember(dest => dest.TrackingNumber, opt => opt.Ignore())
                .ForMember(dest => dest.EstimatedDeliveryDate, opt => opt.Ignore())
                .ForMember(dest => dest.ActualDeliveryDate, opt => opt.Ignore())
                .ForMember(dest => dest.StatusHistory, opt => opt.Ignore());
        }

        private static string GetStatusDisplayName(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "در انتظار",
                OrderStatus.Processing => "در حال پردازش",
                OrderStatus.Packed => "بسته‌بندی شده",
                OrderStatus.Shipped => "ارسال شده",
                OrderStatus.OutForDelivery => "در حال تحویل",
                OrderStatus.Delivered => "تحویل داده شده",
                OrderStatus.Cancelled => "لغو شده",
                OrderStatus.Returned => "مرجوع شده",
                _ => status.ToString()
            };
        }
    }
}
