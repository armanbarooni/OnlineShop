using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Command.Update
{
    public class UpdateProductCategoryCommandHandler
        : IRequestHandler<UpdateProductCategoryCommand, Result<ProductCategoryDto>>
    {
        private readonly IProductCategoryRepository _repository;

        public UpdateProductCategoryCommandHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductCategoryDto>> Handle(
            UpdateProductCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var productCategory = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (productCategory == null)
                return Result<ProductCategoryDto>.Failure($"ProductCategory with Id {request.Id} not found");

            productCategory.Update(
                request.Dto.Name,
                request.Dto.Description,
                null
            );

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
