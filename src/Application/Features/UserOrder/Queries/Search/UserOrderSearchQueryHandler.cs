using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Common;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.Search
{
    public class UserOrderSearchQueryHandler : IRequestHandler<UserOrderSearchQuery, Result<PagedResultDto<UserOrderDto>>>
    {
        private readonly IUserOrderRepository _repository;
        private readonly IMapper _mapper;

        public UserOrderSearchQueryHandler(IUserOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<PagedResultDto<UserOrderDto>>> Handle(UserOrderSearchQuery request, CancellationToken cancellationToken)
        {
            var criteria = request.Criteria ?? new UserOrderSearchCriteriaDto();

            // Get all orders
            var allOrders = await _repository.GetAllAsync(cancellationToken);

            // Apply filters
            var query = allOrders.AsQueryable();

            // Filter by user ID
            if (criteria.UserId.HasValue)
            {
                query = query.Where(o => o.UserId == criteria.UserId.Value);
            }

            // Filter by order status
            if (!string.IsNullOrWhiteSpace(criteria.OrderStatus))
            {
                query = query.Where(o => o.OrderStatus.Equals(criteria.OrderStatus, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by date range
            if (criteria.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= criteria.StartDate.Value);
            }

            if (criteria.EndDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= criteria.EndDate.Value);
            }

            // Filter by amount range
            if (criteria.MinAmount.HasValue)
            {
                query = query.Where(o => o.TotalAmount >= criteria.MinAmount.Value);
            }

            if (criteria.MaxAmount.HasValue)
            {
                query = query.Where(o => o.TotalAmount <= criteria.MaxAmount.Value);
            }

            // Apply sorting
            query = criteria.SortBy?.ToLower() switch
            {
                "totalamount" => criteria.SortDescending
                    ? query.OrderByDescending(o => o.TotalAmount)
                    : query.OrderBy(o => o.TotalAmount),
                "orderdate" => criteria.SortDescending
                    ? query.OrderByDescending(o => o.CreatedAt)
                    : query.OrderBy(o => o.CreatedAt),
                _ => query.OrderByDescending(o => o.CreatedAt) // Default sort by newest
            };

            // Get total count
            var totalCount = query.Count();

            // Apply pagination
            var orders = query
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToList();

            var orderDtos = _mapper.Map<List<UserOrderDto>>(orders);

            var pagedResult = PagedResultDto<UserOrderDto>.Create(
                orderDtos,
                totalCount,
                criteria.PageNumber,
                criteria.PageSize
            );

            return Result<PagedResultDto<UserOrderDto>>.Success(pagedResult);
        }
    }
}

