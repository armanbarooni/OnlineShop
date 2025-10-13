using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.GetPendingRequests
{
    public class GetPendingUserReturnRequestsQueryHandler(
        IUserReturnRequestRepository repository,
        IMapper mapper) : IRequestHandler<GetPendingUserReturnRequestsQuery, Result<List<UserReturnRequestDto>>>
    {
        public async Task<Result<List<UserReturnRequestDto>>> Handle(GetPendingUserReturnRequestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userReturnRequests = await repository.GetPendingRequestsAsync(cancellationToken);
                var dtoList = mapper.Map<List<UserReturnRequestDto>>(userReturnRequests);
                return Result<List<UserReturnRequestDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<UserReturnRequestDto>>.Failure($"خطا در دریافت درخواست‌های مرجوعی در انتظار: {ex.Message}");
            }
        }
    }
}

