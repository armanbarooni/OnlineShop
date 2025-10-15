using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserOrder.Command.Create
{
    public class CreateUserOrderCommandHandler : IRequestHandler<CreateUserOrderCommand, Result<UserOrderDto>>
    {
        private readonly IUserOrderRepository _orderRepository;
        private readonly IUserOrderItemRepository _orderItemRepository;
        private readonly IMapper _mapper;

        public CreateUserOrderCommandHandler(
            IUserOrderRepository orderRepository,
            IUserOrderItemRepository orderItemRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _mapper = mapper;
        }

        public async Task<Result<UserOrderDto>> Handle(CreateUserOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.UserOrder == null)
                return Result<UserOrderDto>.Failure("UserOrder data is required");

            // Generate order number
            var orderNumber = await _orderRepository.GenerateOrderNumberAsync(cancellationToken);

            var userOrder = Domain.Entities.UserOrder.Create(
                request.UserId,
                orderNumber,
                request.UserOrder.SubTotal,
                request.UserOrder.TaxAmount,
                request.UserOrder.ShippingAmount,
                request.UserOrder.DiscountAmount,
                request.UserOrder.TotalAmount,
                request.UserOrder.Currency
            );

            userOrder.SetNotes(request.UserOrder.Notes);
            userOrder.SetShippingAddress(request.UserOrder.ShippingAddressId);
            userOrder.SetBillingAddress(request.UserOrder.BillingAddressId);

            await _orderRepository.AddAsync(userOrder, cancellationToken);

            // Note: Order items should be added separately through UserOrderItem endpoints
            
            return Result<UserOrderDto>.Success(_mapper.Map<UserOrderDto>(userOrder));
        }
    }
}

