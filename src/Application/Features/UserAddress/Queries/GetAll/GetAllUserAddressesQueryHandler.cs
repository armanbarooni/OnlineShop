using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Queries.GetAll
{
    public class GetAllUserAddressesQueryHandler(
        IUserAddressRepository repository,
        IMapper mapper) : IRequestHandler<GetAllUserAddressesQuery, Result<List<UserAddressDto>>>
    {
        public async Task<Result<List<UserAddressDto>>> Handle(GetAllUserAddressesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userAddresses = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<UserAddressDto>>(userAddresses);
                return Result<List<UserAddressDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<UserAddressDto>>.Failure($"خطا در دریافت لیست آدرس‌های کاربران: {ex.Message}");
            }
        }
    }
}

