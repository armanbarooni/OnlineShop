using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserProfile;

namespace OnlineShop.Application.Features.UserProfile.Queries.GetAll
{
    public class GetAllUserProfilesQueryHandler(
        IUserProfileRepository repository,
        IMapper mapper) : IRequestHandler<GetAllUserProfilesQuery, Result<List<UserProfileDto>>>
    {
        public async Task<Result<List<UserProfileDto>>> Handle(GetAllUserProfilesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userProfiles = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<UserProfileDto>>(userProfiles);
                return Result<List<UserProfileDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<UserProfileDto>>.Failure($"خطا در دریافت لیست پروفایل‌های کاربران: {ex.Message}");
            }
        }
    }
}

