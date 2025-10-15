using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserReturnRequest.Command.Delete
{
    public class DeleteUserReturnRequestCommandHandler(IUserReturnRequestRepository repository) 
        : IRequestHandler<DeleteUserReturnRequestCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteUserReturnRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userReturnRequest = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (userReturnRequest == null)
                    return Result<bool>.Failure("درخواست مرجوعی یافت نشد");

                userReturnRequest.Delete(null);
                await repository.UpdateAsync(userReturnRequest, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف درخواست مرجوعی: {ex.Message}");
            }
        }
    }
}



