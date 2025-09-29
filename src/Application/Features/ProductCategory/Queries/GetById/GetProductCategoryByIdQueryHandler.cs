using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Exceptions;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetById
{
    public class GetProductCategoryByIdQueryHandler
    {
        private readonly IProductCategoryRepository _repository;

        public GetProductCategoryByIdQueryHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductCategoryDetailsDto>> Handle(GetProductCategoryByIdQuery query, CancellationToken cancellationToken)
        {
            var productCategory = await _repository.GetByIdAsync(query.Id, cancellationToken);
            if (productCategory == null)
                throw new NotFoundException($"ProductCategory with ID {query.Id} not found.");

            var dto = new ProductCategoryDetailsDto
            {
                Id = productCategory.Id,
                Name = productCategory.Name,
                Description = productCategory.Description,
                MahakId = productCategory.MahakId,
                MahakClientId = productCategory.MahakClientId,
                CreatedAt = productCategory.CreatedAt,
                UpdatedAt = productCategory.UpdatedAt
            };

            return Result<ProductCategoryDetailsDto>.Success(dto);
        }
    }
}
