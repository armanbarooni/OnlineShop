using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Command.Reject
{
    public class RejectUserReturnRequestCommandHandler(
        IUserReturnRequestRepository repository,
        IMapper mapper) : IRequestHandler<RejectUserReturnRequestCommand, Result<UserReturnRequestDto>>
    {
        public async Task<Result<UserReturnRequestDto>> Handle(RejectUserReturnRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userReturnRequest = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (userReturnRequest == null)
                    return Result<UserReturnRequestDto>.Failure("درخواست مرجوعی یافت نشد");

                userReturnRequest.Reject(null, request.RejectionReason, request.AdminNotes);
                await repository.UpdateAsync(userReturnRequest, cancellationToken);
                
                return Result<UserReturnRequestDto>.Success(mapper.Map<UserReturnRequestDto>(userReturnRequest));
            }
            catch (Exception ex)
            {
                return Result<UserReturnRequestDto>.Failure($"خطا در رد درخواست مرجوعی: {ex.Message}");
            }
        }
    }
}

