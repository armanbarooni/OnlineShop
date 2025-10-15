using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductCategory;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductCategory.Queries.GetAll
{
    public class GetAllProductCategoriesQueryHandler
        : IRequestHandler<GetAllProductCategoriesQuery, Result<List<ProductCategoryDto>>>
    {
        private readonly IProductCategoryRepository _repository;

        public GetAllProductCategoriesQueryHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<ProductCategoryDto>>> Handle(
            GetAllProductCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            var categories = await _repository.GetAllAsync(cancellationToken);

            var dtos = categories.Select(c => new ProductCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                MahakId = c.MahakId,
                MahakClientId = c.MahakClientId
            }).ToList();

            return Result<List<ProductCategoryDto>>.Success(dtos);
        }
    }
}


