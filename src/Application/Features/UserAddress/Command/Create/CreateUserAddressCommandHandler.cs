using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserAddress;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.UserAddress.Command.Create
{
    public class CreateUserAddressCommandHandler : IRequestHandler<CreateUserAddressCommand, Result<UserAddressDto>>
    {
        private readonly IUserAddressRepository _repository;
        private readonly IMapper _mapper;

        public CreateUserAddressCommandHandler(IUserAddressRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserAddressDto>> Handle(CreateUserAddressCommand request, CancellationToken cancellationToken)
        {
            if (request.UserAddress == null)
                return Result<UserAddressDto>.Failure("UserAddress data is required");

            var userAddress = Domain.Entities.UserAddress.Create(
                request.UserId,
                request.UserAddress.Title,
                request.UserAddress.FirstName,
                request.UserAddress.LastName,
                request.UserAddress.AddressLine1,
                request.UserAddress.City,
                request.UserAddress.State,
                request.UserAddress.PostalCode,
                request.UserAddress.Country
            );

            userAddress.SetAddressLine2(request.UserAddress.AddressLine2);
            userAddress.SetPhoneNumber(request.UserAddress.PhoneNumber);
            
            if (request.UserAddress.IsDefault)
                userAddress.SetAsDefault();
            
            userAddress.SetAsBillingAddress(request.UserAddress.IsBillingAddress);
            userAddress.SetAsShippingAddress(request.UserAddress.IsShippingAddress);

            await _repository.AddAsync(userAddress, cancellationToken);
            return Result<UserAddressDto>.Success(_mapper.Map<UserAddressDto>(userAddress));
        }
    }
}
