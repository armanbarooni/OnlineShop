using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Features.User.Commands.CreateAddress;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.User.Commands.CreateAddress
{
    public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, Result<Guid>>
    {
        private readonly IUserAddressRepository _addressRepository;

        public CreateAddressCommandHandler(IUserAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<Result<Guid>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            // Split RecipientName into First and Last Name
            string firstName = request.Request.RecipientName;
            string lastName = "-";
            
            if (!string.IsNullOrWhiteSpace(request.Request.RecipientName))
            {
                var parts = request.Request.RecipientName.Trim().Split(' ', 2);
                if (parts.Length > 0) firstName = parts[0];
                if (parts.Length > 1) lastName = parts[1];
            }

            // Create via Entity Factory
            var address = OnlineShop.Domain.Entities.UserAddress.Create(
                request.UserId,
                request.Request.Title,
                firstName,
                lastName,
                request.Request.FullAddress, // AddressLine1
                request.Request.City,
                request.Request.Provincial, // State
                request.Request.PostalCode,
                "Iran", // Country
                null, // AddressLine2
                request.Request.PhoneNumber
            );

            await _addressRepository.AddAsync(address, cancellationToken);
            return Result<Guid>.Success(address.Id);
        }
    }
}
