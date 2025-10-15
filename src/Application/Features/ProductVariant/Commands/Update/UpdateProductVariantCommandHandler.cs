using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductVariant;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductVariant.Commands.Update
{
    public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, Result<ProductVariantDto>>
    {
        private readonly IProductVariantRepository _repository;
        private readonly IMapper _mapper;

        public UpdateProductVariantCommandHandler(IProductVariantRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductVariantDto>> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var productVariant = await _repository.GetByIdAsync(request.ProductVariant.Id, cancellationToken);
                if (productVariant == null)
                {
                    return Result<ProductVariantDto>.Failure("تنوع محصول مورد نظر یافت نشد");
                }

                // Check if another variant with same SKU exists (excluding current one)
                var existingVariant = await _repository.ExistsBySkuAsync(request.ProductVariant.SKU, cancellationToken);
                if (existingVariant)
                {
                    // Get the existing variant to check if it's the same one
                    var allVariants = await _repository.GetAllAsync(cancellationToken);
                    var variantWithSameSku = allVariants.FirstOrDefault(v => v.SKU == request.ProductVariant.SKU && v.Id != request.ProductVariant.Id);
                    if (variantWithSameSku != null)
                    {
                        return Result<ProductVariantDto>.Failure("تنوع محصول با این SKU قبلاً وجود دارد");
                    }
                }

                // Update product variant
                productVariant.Update(
                    request.ProductVariant.Size,
                    request.ProductVariant.Color,
                    request.ProductVariant.SKU,
                    request.ProductVariant.StockQuantity,
                    request.ProductVariant.AdditionalPrice,
                    "System");

                // Set optional properties
                if (!string.IsNullOrEmpty(request.ProductVariant.Barcode))
                    productVariant.SetBarcode(request.ProductVariant.Barcode);

                if (request.ProductVariant.DisplayOrder > 0)
                    productVariant.SetDisplayOrder(request.ProductVariant.DisplayOrder);

                await _repository.UpdateAsync(productVariant, cancellationToken);

                var productVariantDto = _mapper.Map<ProductVariantDto>(productVariant);
                return Result<ProductVariantDto>.Success(productVariantDto);
            }
            catch (Exception ex)
            {
                return Result<ProductVariantDto>.Failure($"خطا در به‌روزرسانی تنوع محصول: {ex.Message}");
            }
        }
    }
}


