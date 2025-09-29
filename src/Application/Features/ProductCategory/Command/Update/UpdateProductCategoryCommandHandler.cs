using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Exceptions;

namespace OnlineShop.Application.Features.ProductCategory.Command.Update
{
    public class UpdateProductCategoryCommandHandler
    {
        private readonly IProductCategoryRepository _repository;

        public UpdateProductCategoryCommandHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductCategoryDto>> Handle(UpdateProductCategoryCommand command, CancellationToken cancellationToken)
        {
            var productCategory = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (productCategory == null)
                throw new NotFoundException($"ProductCategory with ID {command.Id} not found.");

            productCategory.Update(command.Dto.Name, command.Dto.Description, null);
            await _repository.UpdateAsync(productCategory, cancellationToken);
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
