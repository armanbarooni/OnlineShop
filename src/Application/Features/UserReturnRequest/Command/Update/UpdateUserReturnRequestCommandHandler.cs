using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Command.Update
{
    public class UpdateUserReturnRequestCommandHandler(
        IUserReturnRequestRepository repository,
        IMapper mapper) : IRequestHandler<UpdateUserReturnRequestCommand, Result<UserReturnRequestDto>>
    {
        public async Task<Result<UserReturnRequestDto>> Handle(UpdateUserReturnRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.UserReturnRequest == null)
                return Result<UserReturnRequestDto>.Failure("داده‌های درخواست مرجوعی نباید خالی باشد");

            try
            {
                var userReturnRequest = await repository.GetByIdAsync(request.UserReturnRequest.Id, cancellationToken);
                
                if (userReturnRequest == null)
                    return Result<UserReturnRequestDto>.Failure("درخواست مرجوعی یافت نشد");

                userReturnRequest.Update(
                    request.UserReturnRequest.ReturnReason,
                    request.UserReturnRequest.Description,
                    request.UserReturnRequest.Quantity,
                    request.UserReturnRequest.RefundAmount,
                    request.UserReturnRequest.UpdatedBy
                );

                await repository.UpdateAsync(userReturnRequest, cancellationToken);
                return Result<UserReturnRequestDto>.Success(mapper.Map<UserReturnRequestDto>(userReturnRequest));
            }
            catch (Exception ex)
            {
                return Result<UserReturnRequestDto>.Failure($"خطا در به‌روزرسانی درخواست مرجوعی: {ex.Message}");
            }
        }
    }
}

