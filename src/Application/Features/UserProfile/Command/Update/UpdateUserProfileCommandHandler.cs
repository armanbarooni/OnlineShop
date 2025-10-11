using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserProfile;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.UserProfile.Command.Update
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
    {
        private readonly IUserProfileRepository _repository;
        private readonly IMapper _mapper;

        public UpdateUserProfileCommandHandler(IUserProfileRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserProfileDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            if (request.UserProfile == null)
                return Result<UserProfileDto>.Failure("UserProfile data is required");

            var userProfile = await _repository.GetByIdAsync(request.UserProfile.Id, cancellationToken);
            if (userProfile == null)
                return Result<UserProfileDto>.Failure("UserProfile not found");

            userProfile.Update(
                request.UserProfile.FirstName,
                request.UserProfile.LastName,
                request.UserProfile.NationalCode,
                request.UserProfile.BirthDate,
                request.UserProfile.Gender,
                request.UserProfile.ProfileImageUrl,
                request.UserProfile.Bio,
                request.UserProfile.Website,
                null
            );

            await _repository.UpdateAsync(userProfile, cancellationToken);
            return Result<UserProfileDto>.Success(_mapper.Map<UserProfileDto>(userProfile));
        }
    }
}
