using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserReturnRequest;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserReturnRequest.Command.Approve
{
    public class ApproveUserReturnRequestCommandHandler(
        IUserReturnRequestRepository repository,
        IMapper mapper) : IRequestHandler<ApproveUserReturnRequestCommand, Result<UserReturnRequestDto>>
    {
        public async Task<Result<UserReturnRequestDto>> Handle(ApproveUserReturnRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userReturnRequest = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (userReturnRequest == null)
                    return Result<UserReturnRequestDto>.Failure("درخواست مرجوعی یافت نشد");

                userReturnRequest.Approve(null, request.AdminNotes);
                await repository.UpdateAsync(userReturnRequest, cancellationToken);
                
                return Result<UserReturnRequestDto>.Success(mapper.Map<UserReturnRequestDto>(userReturnRequest));
            }
            catch (Exception ex)
            {
                return Result<UserReturnRequestDto>.Failure($"خطا در تایید درخواست مرجوعی: {ex.Message}");
            }
        }
    }
}



