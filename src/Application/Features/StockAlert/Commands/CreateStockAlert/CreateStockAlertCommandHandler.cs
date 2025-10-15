using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.StockAlert;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.StockAlert.Commands.CreateStockAlert
{
    public class CreateStockAlertCommandHandler : IRequestHandler<CreateStockAlertCommand, Result<StockAlertResultDto>>
    {
        private readonly IStockAlertRepository _stockAlertRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;

        public CreateStockAlertCommandHandler(
            IStockAlertRepository stockAlertRepository,
            IProductRepository productRepository,
            IProductVariantRepository productVariantRepository)
        {
            _stockAlertRepository = stockAlertRepository;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
        }

        public async Task<Result<StockAlertResultDto>> Handle(CreateStockAlertCommand request, CancellationToken cancellationToken)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                return Result<StockAlertResultDto>.Failure("محصول یافت نشد");

            // Validate product variant if provided
            if (request.ProductVariantId.HasValue)
            {
                var productVariant = await _productVariantRepository.GetByIdAsync(request.ProductVariantId.Value, cancellationToken);
                if (productVariant == null)
                    return Result<StockAlertResultDto>.Failure("تنوع محصول یافت نشد");
            }

            // Check if alert already exists
            var existingAlert = await _stockAlertRepository.ExistsAsync(
                request.ProductId, 
                request.ProductVariantId, 
                request.UserId, 
                cancellationToken);

            if (existingAlert)
                return Result<StockAlertResultDto>.Failure("هشدار موجودی برای این محصول قبلاً ثبت شده است");

            // Create stock alert
            var stockAlert = Domain.Entities.StockAlert.Create(
                request.ProductId,
                request.ProductVariantId,
                request.UserId,
                request.Email,
                request.PhoneNumber,
                request.NotificationMethod);

            await _stockAlertRepository.AddAsync(stockAlert, cancellationToken);

            var result = new StockAlertResultDto
            {
                Id = stockAlert.Id,
                Message = "هشدار موجودی با موفقیت ثبت شد. هنگام موجود شدن محصول به شما اطلاع داده خواهد شد.",
                Success = true
            };

            return Result<StockAlertResultDto>.Success(result);
        }
    }
}
