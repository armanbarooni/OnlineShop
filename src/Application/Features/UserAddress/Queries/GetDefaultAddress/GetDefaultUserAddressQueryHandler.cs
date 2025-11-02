using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserAddress.Queries.GetDefaultAddress
{
    public class GetDefaultUserAddressQueryHandler : IRequestHandler<GetDefaultUserAddressQuery, Result<UserAddressDto>>
    {
        private readonly IUserAddressRepository _repository;
        private readonly IMapper _mapper;

        public GetDefaultUserAddressQueryHandler(IUserAddressRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserAddressDto>> Handle(GetDefaultUserAddressQuery request, CancellationToken cancellationToken)
        {
            var address = await _repository.GetDefaultAddressAsync(request.UserId, cancellationToken);
            if (address == null)
                return Result<UserAddressDto>.Failure("آدرس پیش‌فرض یافت نشد");

            var dto = _mapper.Map<UserAddressDto>(address);
            return Result<UserAddressDto>.Success(dto);
        }
    }
}

