using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Command.Update
{
    public class UpdateUserOrderCommandHandler : IRequestHandler<UpdateUserOrderCommand, Result<UserOrderDto>>
    {
        private readonly IUserOrderRepository _repository;
        private readonly IMapper _mapper;

        public UpdateUserOrderCommandHandler(IUserOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserOrderDto>> Handle(UpdateUserOrderCommand request, CancellationToken cancellationToken)
        {
            var userOrder = await _repository.GetByIdAsync(request.UserOrder.Id, cancellationToken);
            if (userOrder == null)
                return Result<UserOrderDto>.Failure("UserOrder not found");

            userOrder.Update(
                request.UserOrder.OrderStatus,
                request.UserOrder.TotalAmount,
                request.UserOrder.ShippingCost,
                request.UserOrder.DiscountAmount,
                request.UserOrder.FinalAmount,
                request.UserOrder.PaymentStatus,
                request.UserOrder.ShippingAddress,
                request.UserOrder.Notes,
                request.UserOrder.ShippedDate,
                request.UserOrder.DeliveredDate,
                "System"
            );

            await _repository.UpdateAsync(userOrder, cancellationToken);
            return Result<UserOrderDto>.Success(_mapper.Map<UserOrderDto>(userOrder));
        }
    }
}
