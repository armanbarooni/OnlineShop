using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Application.DTOs.Common;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetByUserId
{
    public class GetUserPaymentsByUserIdQueryHandler : IRequestHandler<GetUserPaymentsByUserIdQuery, Result<PagedResultDto<UserPaymentDto>>>
    {
        private readonly IUserPaymentRepository _repository;
        private readonly IMapper _mapper;

        public GetUserPaymentsByUserIdQueryHandler(IUserPaymentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<PagedResultDto<UserPaymentDto>>> Handle(GetUserPaymentsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var allUserPayments = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            var paymentsList = allUserPayments.ToList();
            
            var totalCount = paymentsList.Count;
            
            // Apply pagination
            var paginatedPayments = paymentsList
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            
            var userPaymentDtos = _mapper.Map<List<UserPaymentDto>>(paginatedPayments);
            
            var pagedResult = PagedResultDto<UserPaymentDto>.Create(
                userPaymentDtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
            
            return Result<PagedResultDto<UserPaymentDto>>.Success(pagedResult);
        }
    }
}


