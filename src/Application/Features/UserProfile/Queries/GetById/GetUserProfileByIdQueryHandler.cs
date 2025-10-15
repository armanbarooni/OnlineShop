using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserProfile;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.UserProfile.Queries.GetById
{
    public class GetUserProfileByIdQueryHandler(
        IUserProfileRepository repository,
        IMapper mapper) : IRequestHandler<GetUserProfileByIdQuery, Result<UserProfileDto>>
    {
        public async Task<Result<UserProfileDto>> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userProfile = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (userProfile == null)
                    return Result<UserProfileDto>.Failure("پروفایل کاربر یافت نشد");

                var dto = mapper.Map<UserProfileDto>(userProfile);
                return Result<UserProfileDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<UserProfileDto>.Failure($"خطا در دریافت پروفایل کاربر: {ex.Message}");
            }
        }
    }
}



