using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserOrder;

using OnlineShop.Domain.Interfaces.Repositories;
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
                request.UserOrder.SubTotal,
                request.UserOrder.TaxAmount,
                request.UserOrder.ShippingAmount,
                request.UserOrder.DiscountAmount,
                request.UserOrder.TotalAmount,
                request.UserOrder.Notes,
                "System"
            );
            
            if (!string.IsNullOrEmpty(request.UserOrder.TrackingNumber))
            {
                userOrder.SetTrackingNumber(request.UserOrder.TrackingNumber);
            }

            await _repository.UpdateAsync(userOrder, cancellationToken);
            return Result<UserOrderDto>.Success(_mapper.Map<UserOrderDto>(userOrder));
        }
    }
}


