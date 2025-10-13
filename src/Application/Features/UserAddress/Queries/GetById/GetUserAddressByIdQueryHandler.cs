using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Queries.GetById
{
    public class GetUserAddressByIdQueryHandler(
        IUserAddressRepository repository,
        IMapper mapper) : IRequestHandler<GetUserAddressByIdQuery, Result<UserAddressDto>>
    {
        public async Task<Result<UserAddressDto>> Handle(GetUserAddressByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userAddress = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (userAddress == null)
                    return Result<UserAddressDto>.Failure("آدرس کاربر یافت نشد");

                var dto = mapper.Map<UserAddressDto>(userAddress);
                return Result<UserAddressDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<UserAddressDto>.Failure($"خطا در دریافت آدرس کاربر: {ex.Message}");
            }
        }
    }
}

