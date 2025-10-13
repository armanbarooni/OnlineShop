using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Application.Services;

namespace OnlineShop.Application.Features.UserOrder.Commands.CancelOrder
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<UserOrderDto>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IMapper _mapper;

        public CancelOrderCommandHandler(
            IUserOrderRepository orderRepository,
            IInventoryService inventoryService,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _inventoryService = inventoryService;
            _mapper = mapper;
        }

        public async Task<Result<UserOrderDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<UserOrderDto>.Failure("سفارش یافت نشد");

            try
            {
                // Cancel the order
                order.Cancel(request.CancellationReason, request.UpdatedBy);

                // Release reserved inventory back to available stock
                await _inventoryService.ReleaseStockForCancelledOrder(request.OrderId, cancellationToken);

                await _orderRepository.UpdateAsync(order, cancellationToken);

                return Result<UserOrderDto>.Success(_mapper.Map<UserOrderDto>(order));
            }
            catch (InvalidOperationException ex)
            {
                return Result<UserOrderDto>.Failure(ex.Message);
            }
        }
    }
}

