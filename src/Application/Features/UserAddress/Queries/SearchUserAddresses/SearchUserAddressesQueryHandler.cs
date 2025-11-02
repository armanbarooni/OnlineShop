using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserAddress.Queries.SearchUserAddresses
{
    public class SearchUserAddressesQueryHandler : IRequestHandler<SearchUserAddressesQuery, Result<IEnumerable<UserAddressDto>>>
    {
        private readonly IUserAddressRepository _repository;
        private readonly IMapper _mapper;

        public SearchUserAddressesQueryHandler(IUserAddressRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserAddressDto>>> Handle(SearchUserAddressesQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                var query = request.SearchQuery.ToLower();
                addresses = addresses.Where(a =>
                    a.Title.ToLower().Contains(query) ||
                    a.FirstName.ToLower().Contains(query) ||
                    a.LastName.ToLower().Contains(query) ||
                    a.AddressLine1.ToLower().Contains(query) ||
                    a.City.ToLower().Contains(query) ||
                    a.State.ToLower().Contains(query) ||
                    a.PostalCode.Contains(query)
                );
            }

            var dtos = _mapper.Map<IEnumerable<UserAddressDto>>(addresses);
            return Result<IEnumerable<UserAddressDto>>.Success(dtos);
        }
    }
}

