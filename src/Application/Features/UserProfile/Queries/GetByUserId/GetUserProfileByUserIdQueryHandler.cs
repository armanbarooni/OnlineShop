using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserProfile;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserProfile.Queries.GetByUserId
{
    public class GetUserProfileByUserIdQueryHandler : IRequestHandler<GetUserProfileByUserIdQuery, Result<UserProfileDto?>>
    {
        private readonly IUserProfileRepository _repository;
        private readonly IMapper _mapper;

        public GetUserProfileByUserIdQueryHandler(IUserProfileRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserProfileDto?>> Handle(GetUserProfileByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userProfile = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (userProfile == null)
                return Result<UserProfileDto?>.Success(null);

            return Result<UserProfileDto?>.Success(_mapper.Map<UserProfileDto>(userProfile));
        }
    }
}

