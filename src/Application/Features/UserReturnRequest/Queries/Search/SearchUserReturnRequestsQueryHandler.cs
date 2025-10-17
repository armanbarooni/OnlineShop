using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.Search
{
    public class SearchUserReturnRequestsQueryHandler : IRequestHandler<SearchUserReturnRequestsQuery, Result<IEnumerable<UserReturnRequestDto>>>
    {
        private readonly IUserReturnRequestRepository _repository;

        public SearchUserReturnRequestsQueryHandler(IUserReturnRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<UserReturnRequestDto>>> Handle(SearchUserReturnRequestsQuery request, CancellationToken cancellationToken)
        {
            var allRequests = await _repository.GetAllAsync(cancellationToken);

            // Filter by status if provided
            if (!string.IsNullOrEmpty(request.Status))
            {
                allRequests = allRequests.Where(r => r.ReturnStatus.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Apply pagination
            var skip = (request.PageNumber - 1) * request.PageSize;
            var paginatedRequests = allRequests
                .Skip(skip)
                .Take(request.PageSize)
                .ToList();

            var dtos = paginatedRequests.Select(r => new UserReturnRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                OrderId = r.OrderId,
                OrderItemId = r.OrderItemId,
                ReturnReason = r.ReturnReason,
                ReturnStatus = r.ReturnStatus,
                Description = r.Description,
                Quantity = r.Quantity,
                RefundAmount = r.RefundAmount,
                AdminNotes = r.AdminNotes,
                ApprovedAt = r.ApprovedAt,
                ApprovedBy = r.ApprovedBy,
                RejectedAt = r.RejectedAt,
                RejectedBy = r.RejectedBy,
                RejectionReason = r.RejectionReason,
                ProcessedAt = r.ProcessedAt,
                ProcessedBy = r.ProcessedBy,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt ?? r.CreatedAt
            }).ToList();

            return Result<IEnumerable<UserReturnRequestDto>>.Success(dtos);
        }
    }
}

