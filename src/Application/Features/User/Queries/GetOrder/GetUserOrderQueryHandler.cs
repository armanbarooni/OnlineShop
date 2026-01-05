using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Order;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.User.Queries.GetOrder
{
    public class GetUserOrderQueryHandler : IRequestHandler<GetUserOrderQuery, Result<OrderDetailDto>>
    {
        private readonly IUserOrderRepository _orderRepository;

        public GetUserOrderQueryHandler(IUserOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<OrderDetailDto>> Handle(GetUserOrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            
            if (order == null || order.UserId != request.UserId)
            {
                return Result<OrderDetailDto>.Failure("سفارش یافت نشد");
            }

            return Result<OrderDetailDto>.Success(new OrderDetailDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                SubTotal = order.SubTotal,
                ShippingAmount = order.ShippingAmount,
                DiscountAmount = order.DiscountAmount,
                CreatedAt = order.CreatedAt,
                TotalItems = order.OrderItems.Count, // Assuming items are loaded
                TrackingNumber = order.TrackingNumber,
                PaymentMethod = order.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.PaymentMethod ?? "Unknown", 
                // Address details ideally loaded from ShippingAddress navigation property if available
                // order.ShippingAddress might be null if not included in repository
                
                    Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductImage = i.Product?.ProductImages.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    VariantInfo = i.VariantId.HasValue ? "Variant Info" : null // Needs product variant include
                }).ToList()
            });
        }
    }
}
