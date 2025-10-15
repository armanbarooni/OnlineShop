using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserAddress.Command.Delete
{
    public class DeleteUserAddressCommandHandler(IUserAddressRepository repository)
        : IRequestHandler<DeleteUserAddressCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userAddress = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (userAddress == null)
                    return Result<bool>.Failure("آدرس کاربر یافت نشد");

                userAddress.Delete(null);
                await repository.UpdateAsync(userAddress, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف آدرس کاربر: {ex.Message}");
            }
        }
    }
}



