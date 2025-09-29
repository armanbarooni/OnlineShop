using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetAll
{
    public class GetAllProductCategoriesQueryHandler
    {
        private readonly IProductCategoryRepository _repository;

        public GetAllProductCategoriesQueryHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IEnumerable<ProductCategoryDto>>> Handle(GetAllProductCategoriesQuery query, CancellationToken cancellationToken)
        {
            var productCategories = await _repository.GetAllAsync(cancellationToken);
            var dtos = productCategories.Select(pc => new ProductCategoryDto
            {
                Id = pc.Id,
                Name = pc.Name,
                Description = pc.Description,
                MahakId = pc.MahakId,
                MahakClientId = pc.MahakClientId
            });

            return Result<IEnumerable<ProductCategoryDto>>.Success(dtos);
        }
    }
}
