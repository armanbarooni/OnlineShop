using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.ProductCategory.Command.Create
{
    public class CreateProductCategoryCommandHandler
    {
        private readonly IProductCategoryRepository _repository;

        public CreateProductCategoryCommandHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductCategoryDto>> Handle(CreateProductCategoryCommand command, CancellationToken cancellationToken)
        {
            var productCategory = Domain.Entities.ProductCategory.Create(
                command.Dto.Name,
                command.Dto.Description,
                command.Dto.MahakClientId,
                command.Dto.MahakId
            );

            await _repository.AddAsync(productCategory, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            var dto = new ProductCategoryDto
            {
                Id = productCategory.Id,
                Name = productCategory.Name,
                Description = productCategory.Description,
                MahakId = productCategory.MahakId,
                MahakClientId = productCategory.MahakClientId
            };

            return Result<ProductCategoryDto>.Success(dto);
        }
    }
}
