using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.UserProfile;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserProfile.Command.Create
{
    public class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommand, Result<UserProfileDto>>
    {
        private readonly IUserProfileRepository _repository;
        private readonly IMapper _mapper;

        public CreateUserProfileCommandHandler(IUserProfileRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserProfileDto>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
        {
            if (request.UserProfile == null)
                return Result<UserProfileDto>.Failure("UserProfile data is required");

            var userProfile = Domain.Entities.UserProfile.Create(
                request.UserProfile.UserId,
                request.UserProfile.FirstName,
                request.UserProfile.LastName
            );

            userProfile.SetNationalCode(request.UserProfile.NationalCode);
            userProfile.SetBirthDate(request.UserProfile.BirthDate);
            userProfile.SetGender(request.UserProfile.Gender);
            userProfile.SetProfileImageUrl(request.UserProfile.ProfileImageUrl);
            userProfile.SetBio(request.UserProfile.Bio);
            userProfile.SetWebsite(request.UserProfile.Website);

            await _repository.AddAsync(userProfile, cancellationToken);
            return Result<UserProfileDto>.Success(_mapper.Map<UserProfileDto>(userProfile));
        }
    }
}

