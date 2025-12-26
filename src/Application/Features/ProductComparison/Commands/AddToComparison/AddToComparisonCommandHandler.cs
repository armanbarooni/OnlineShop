using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductComparison;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductComparison.Commands.AddToComparison
{
    public class AddToComparisonCommandHandler : IRequestHandler<AddToComparisonCommand, Result<ProductComparisonDto>>
    {
        private readonly IProductComparisonRepository _repository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public AddToComparisonCommandHandler(
            IProductComparisonRepository repository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _repository = repository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<ProductComparisonDto>> Handle(AddToComparisonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify product exists
                var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
                if (product == null)
                    return Result<ProductComparisonDto>.Failure("Product not found");

                // Get or create comparison
                var comparison = await _repository.GetByUserIdAsync(Guid.Parse(request.UserId), cancellationToken);
                if (comparison == null)
                {
                    comparison = Domain.Entities.ProductComparison.Create(Guid.Parse(request.UserId));
                    await _repository.AddAsync(comparison, cancellationToken);
                }

                // Add product to comparison
                comparison.AddProduct(request.ProductId);
                await _repository.UpdateAsync(comparison, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                var dto = _mapper.Map<ProductComparisonDto>(comparison);
                return Result<ProductComparisonDto>.Success(dto);
            }
            catch (InvalidOperationException ex)
            {
                return Result<ProductComparisonDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<ProductComparisonDto>.Failure($"Error adding product to comparison: {ex.Message}");
            }
        }
    }
}

