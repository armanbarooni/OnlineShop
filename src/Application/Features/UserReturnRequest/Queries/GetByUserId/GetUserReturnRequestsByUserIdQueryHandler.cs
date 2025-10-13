using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.GetByUserId
{
    public class GetUserReturnRequestsByUserIdQueryHandler(
        IUserReturnRequestRepository repository,
        IMapper mapper) : IRequestHandler<GetUserReturnRequestsByUserIdQuery, Result<List<UserReturnRequestDto>>>
    {
        public async Task<Result<List<UserReturnRequestDto>>> Handle(GetUserReturnRequestsByUserIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userReturnRequests = await repository.GetByUserIdAsync(request.UserId, cancellationToken);
                var dtoList = mapper.Map<List<UserReturnRequestDto>>(userReturnRequests);
                return Result<List<UserReturnRequestDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<UserReturnRequestDto>>.Failure($"خطا در دریافت درخواست‌های مرجوعی کاربر: {ex.Message}");
            }
        }
    }
}

