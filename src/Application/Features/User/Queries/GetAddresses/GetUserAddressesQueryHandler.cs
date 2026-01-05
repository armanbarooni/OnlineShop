using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.User;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.User.Queries.GetAddresses
{
    public class GetUserAddressesQueryHandler : IRequestHandler<GetUserAddressesQuery, Result<List<UserAddressDto>>>
    {
        private readonly IUserAddressRepository _addressRepository;

        public GetUserAddressesQueryHandler(IUserAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<Result<List<UserAddressDto>>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            
            var dtos = addresses.Select(a => new UserAddressDto
            {
                Id = a.Id,
                Title = a.Title,
                RecipientName = $"{a.FirstName} {a.LastName}",
                PhoneNumber = a.PhoneNumber ?? "",
                Provincial = a.State, // Map Province to State
                City = a.City,
                FullAddress = a.AddressLine1, // Map FullAddress to AddressLine1
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault
            }).ToList();

            return Result<List<UserAddressDto>>.Success(dtos);
        }
    }
}
