using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductComparison.Commands.ClearComparison
{
    public class ClearComparisonCommandHandler : IRequestHandler<ClearComparisonCommand, Result<bool>>
    {
        private readonly IProductComparisonRepository _repository;

        public ClearComparisonCommandHandler(IProductComparisonRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(ClearComparisonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var comparison = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
                if (comparison == null)
                    return Result<bool>.Failure("Comparison not found");

                comparison.Clear();
                await _repository.UpdateAsync(comparison, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error clearing comparison: {ex.Message}");
            }
        }
    }
}

