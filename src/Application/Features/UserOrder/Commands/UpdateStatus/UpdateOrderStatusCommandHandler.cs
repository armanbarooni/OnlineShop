using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserOrder;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserOrder.Commands.UpdateStatus
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<UserOrderDto>>
    {
        private readonly IUserOrderRepository _repository;
        private readonly IMapper _mapper;

        public UpdateOrderStatusCommandHandler(IUserOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserOrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _repository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<UserOrderDto>.Failure("سفارش یافت نشد");

            try
            {
                switch (request.NewStatus.ToLower())
                {
                    case "processing":
                        order.StartProcessing(request.UpdatedBy);
                        break;
                    case "shipped":
                        if (string.IsNullOrEmpty(request.TrackingNumber))
                            return Result<UserOrderDto>.Failure("کد رهگیری برای ارسال الزامی است");
                        order.MarkAsShipped(request.TrackingNumber, request.UpdatedBy);
                        break;
                    case "delivered":
                        order.MarkAsDelivered(request.UpdatedBy);
                        break;
                    default:
                        return Result<UserOrderDto>.Failure($"وضعیت نامعتبر: {request.NewStatus}");
                }

                await _repository.UpdateAsync(order, cancellationToken);
                
                return Result<UserOrderDto>.Success(_mapper.Map<UserOrderDto>(order));
            }
            catch (InvalidOperationException ex)
            {
                return Result<UserOrderDto>.Failure(ex.Message);
            }
        }
    }
}



