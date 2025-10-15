using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductVariant;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.ProductVariant.Commands.Create
{
    public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, Result<ProductVariantDto>>
    {
        private readonly IProductVariantRepository _repository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CreateProductVariantCommandHandler(
            IProductVariantRepository repository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _repository = repository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<ProductVariantDto>> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if product exists
                var product = await _productRepository.GetByIdAsync(request.ProductVariant.ProductId, cancellationToken);
                if (product == null)
                {
                    return Result<ProductVariantDto>.Failure("محصول مورد نظر یافت نشد");
                }

                // Check if variant with same SKU already exists
                var existingVariant = await _repository.ExistsBySkuAsync(request.ProductVariant.SKU, cancellationToken);
                if (existingVariant)
                {
                    return Result<ProductVariantDto>.Failure("تنوع محصول با این SKU قبلاً وجود دارد");
                }

                // Create new product variant
                var productVariant = Domain.Entities.ProductVariant.Create(
                    request.ProductVariant.ProductId,
                    request.ProductVariant.Size,
                    request.ProductVariant.Color,
                    request.ProductVariant.SKU,
                    request.ProductVariant.StockQuantity);

                // Set optional properties
                if (!string.IsNullOrEmpty(request.ProductVariant.Barcode))
                    productVariant.SetBarcode(request.ProductVariant.Barcode);

                if (request.ProductVariant.AdditionalPrice.HasValue)
                    productVariant.SetAdditionalPrice(request.ProductVariant.AdditionalPrice);

                if (request.ProductVariant.DisplayOrder > 0)
                    productVariant.SetDisplayOrder(request.ProductVariant.DisplayOrder);

                await _repository.AddAsync(productVariant, cancellationToken);

                var productVariantDto = _mapper.Map<ProductVariantDto>(productVariant);
                return Result<ProductVariantDto>.Success(productVariantDto);
            }
            catch (Exception ex)
            {
                return Result<ProductVariantDto>.Failure($"خطا در ایجاد تنوع محصول: {ex.Message}");
            }
        }
    }
}
