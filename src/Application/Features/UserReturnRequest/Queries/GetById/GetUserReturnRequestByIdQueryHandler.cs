using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.GetById
{
    public class GetUserReturnRequestByIdQueryHandler(
        IUserReturnRequestRepository repository,
        IMapper mapper) : IRequestHandler<GetUserReturnRequestByIdQuery, Result<UserReturnRequestDto>>
    {
        public async Task<Result<UserReturnRequestDto>> Handle(GetUserReturnRequestByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userReturnRequest = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (userReturnRequest == null)
                    return Result<UserReturnRequestDto>.Failure("درخواست مرجوعی یافت نشد");

                var dto = mapper.Map<UserReturnRequestDto>(userReturnRequest);
                return Result<UserReturnRequestDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<UserReturnRequestDto>.Failure($"خطا در دریافت درخواست مرجوعی: {ex.Message}");
            }
        }
    }
}

