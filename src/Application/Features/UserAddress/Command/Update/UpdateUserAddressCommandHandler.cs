using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserAddress;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.UserAddress.Command.Update
{
    public class UpdateUserAddressCommandHandler : IRequestHandler<UpdateUserAddressCommand, Result<UserAddressDto>>
    {
        private readonly IUserAddressRepository _repository;
        private readonly IMapper _mapper;

        public UpdateUserAddressCommandHandler(IUserAddressRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserAddressDto>> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
        {
            if (request.UserAddress == null)
                return Result<UserAddressDto>.Failure("UserAddress data is required");

            var userAddress = await _repository.GetByIdAsync(request.UserAddress.Id, cancellationToken);
            if (userAddress == null)
                return Result<UserAddressDto>.Failure("UserAddress not found");

            userAddress.Update(
                request.UserAddress.Title,
                request.UserAddress.FirstName,
                request.UserAddress.LastName,
                request.UserAddress.AddressLine1,
                request.UserAddress.AddressLine2,
                request.UserAddress.City,
                request.UserAddress.State,
                request.UserAddress.PostalCode,
                request.UserAddress.Country,
                request.UserAddress.PhoneNumber,
                request.UserAddress.IsDefault,
                request.UserAddress.IsBillingAddress,
                request.UserAddress.IsShippingAddress,
                null
            );

            await _repository.UpdateAsync(userAddress, cancellationToken);
            return Result<UserAddressDto>.Success(_mapper.Map<UserAddressDto>(userAddress));
        }
    }
}
