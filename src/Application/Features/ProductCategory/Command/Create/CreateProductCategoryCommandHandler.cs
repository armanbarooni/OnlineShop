using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductCategory;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductCategory.Command.Create
{
    public class CreateProductCategoryCommandHandler(IProductCategoryRepository repository)
                : IRequestHandler<CreateProductCategoryCommand, Result<ProductCategoryDto>>
    {
        private readonly IProductCategoryRepository _repository = repository;

        public async Task<Result<ProductCategoryDto>> Handle(
            CreateProductCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var productCategory = Domain.Entities.ProductCategory.Create(
                request.Dto.Name,
                request.Dto.Description,
                request.Dto.MahakClientId,
                request.Dto.MahakId
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


