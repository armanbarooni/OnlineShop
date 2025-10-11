using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserAddress;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.UserAddress.Queries.GetByUserId
{
    public class GetUserAddressesByUserIdQueryHandler : IRequestHandler<GetUserAddressesByUserIdQuery, Result<IEnumerable<UserAddressDto>>>
    {
        private readonly IUserAddressRepository _repository;
        private readonly IMapper _mapper;

        public GetUserAddressesByUserIdQueryHandler(IUserAddressRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserAddressDto>>> Handle(GetUserAddressesByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userAddresses = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            var userAddressDtos = _mapper.Map<IEnumerable<UserAddressDto>>(userAddresses);
            return Result<IEnumerable<UserAddressDto>>.Success(userAddressDtos);
        }
    }
}
