using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserProfile.Command.Delete
{
    public class DeleteUserProfileCommandHandler(IUserProfileRepository repository)
        : IRequestHandler<DeleteUserProfileCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userProfile = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (userProfile == null)
                    return Result<bool>.Failure("پروفایل کاربر یافت نشد");

                userProfile.Delete(null);
                await repository.UpdateAsync(userProfile, cancellationToken);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطا در حذف پروفایل کاربر: {ex.Message}");
            }
        }
    }
}



