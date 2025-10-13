using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Command.Create
{
    public class CreateUserReturnRequestCommandHandler(
        IUserReturnRequestRepository repository, 
        IMapper mapper) : IRequestHandler<CreateUserReturnRequestCommand, Result<UserReturnRequestDto>>
    {
        public async Task<Result<UserReturnRequestDto>> Handle(CreateUserReturnRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.UserReturnRequest == null)
                return Result<UserReturnRequestDto>.Failure("داده‌های درخواست مرجوعی نباید خالی باشد");

            try
            {
                var userReturnRequest = Domain.Entities.UserReturnRequest.Create(
                    request.UserReturnRequest.UserId,
                    request.UserReturnRequest.OrderId,
                    request.UserReturnRequest.OrderItemId,
                    request.UserReturnRequest.ReturnReason,
                    request.UserReturnRequest.Description,
                    request.UserReturnRequest.Quantity,
                    request.UserReturnRequest.RefundAmount
                );

                await repository.AddAsync(userReturnRequest, cancellationToken);
                return Result<UserReturnRequestDto>.Success(mapper.Map<UserReturnRequestDto>(userReturnRequest));
            }
            catch (Exception ex)
            {
                return Result<UserReturnRequestDto>.Failure($"خطا در ایجاد درخواست مرجوعی: {ex.Message}");
            }
        }
    }
}

