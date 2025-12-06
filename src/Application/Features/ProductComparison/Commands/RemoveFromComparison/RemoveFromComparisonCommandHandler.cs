using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductComparison.Commands.RemoveFromComparison
{
    public class RemoveFromComparisonCommandHandler : IRequestHandler<RemoveFromComparisonCommand, Result<bool>>
    {
        private readonly IProductComparisonRepository _repository;

        public RemoveFromComparisonCommandHandler(IProductComparisonRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(RemoveFromComparisonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var comparison = await _repository.GetByUserIdAsync(Guid.Parse(request.UserId), cancellationToken);
                if (comparison == null)
                    return Result<bool>.Failure("Comparison not found");

                comparison.RemoveProduct(request.ProductId);
                await _repository.UpdateAsync(comparison, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (InvalidOperationException ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error removing product from comparison: {ex.Message}");
            }
        }
    }
}

