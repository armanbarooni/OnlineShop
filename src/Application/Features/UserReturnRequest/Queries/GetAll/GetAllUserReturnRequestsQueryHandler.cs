using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.GetAll
{
    public class GetAllUserReturnRequestsQueryHandler(
        IUserReturnRequestRepository repository,
        IMapper mapper) : IRequestHandler<GetAllUserReturnRequestsQuery, Result<List<UserReturnRequestDto>>>
    {
        public async Task<Result<List<UserReturnRequestDto>>> Handle(GetAllUserReturnRequestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userReturnRequests = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<UserReturnRequestDto>>(userReturnRequests);
                return Result<List<UserReturnRequestDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<UserReturnRequestDto>>.Failure($"خطا در دریافت لیست درخواست‌های مرجوعی: {ex.Message}");
            }
        }
    }
}

