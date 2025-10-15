using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductCategory;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductCategory.Queries.GetById
{
    public class GetProductCategoryByIdQueryHandler
        : IRequestHandler<GetProductCategoryByIdQuery, Result<ProductCategoryDto>>
    {
        private readonly IProductCategoryRepository _repository;

        public GetProductCategoryByIdQueryHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductCategoryDto>> Handle(
            GetProductCategoryByIdQuery request,
            CancellationToken cancellationToken)
        {
            var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (category == null)
                return Result<ProductCategoryDto>.Failure($"ProductCategory with Id {request.Id} not found");

            var dto = new ProductCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                MahakId = category.MahakId,
                MahakClientId = category.MahakClientId
            };

            return Result<ProductCategoryDto>.Success(dto);
        }
    }
}


