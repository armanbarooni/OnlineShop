using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Checkout.Commands.ValidateCheckout
{
    public class ValidateCheckoutCommandHandler : IRequestHandler<ValidateCheckoutCommand, Result<CheckoutValidationResultDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductInventoryRepository _inventoryRepository;
        private readonly IUserAddressRepository _addressRepository;
        private readonly ICouponRepository _couponRepository;

        public ValidateCheckoutCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IProductInventoryRepository inventoryRepository,
            IUserAddressRepository addressRepository,
            ICouponRepository couponRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _addressRepository = addressRepository;
            _couponRepository = couponRepository;
        }

        public async Task<Result<CheckoutValidationResultDto>> Handle(ValidateCheckoutCommand request, CancellationToken cancellationToken)
        {
            var validationResult = new CheckoutValidationResultDto { IsValid = true };

            // 1. Validate cart
            var cart = await _cartRepository.GetByIdAsync(request.Request.CartId, cancellationToken);
            if (cart == null)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("سبد خرید یافت نشد");
                return Result<CheckoutValidationResultDto>.Success(validationResult);
            }

            if (cart.UserId != request.UserId)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("دسترسی به این سبد خرید مجاز نیست");
                return Result<CheckoutValidationResultDto>.Success(validationResult);
            }

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id, cancellationToken);
            if (!cartItems.Any())
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("سبد خرید خالی است");
                return Result<CheckoutValidationResultDto>.Success(validationResult);
            }

            // 2. Validate shipping address
            var shippingAddress = await _addressRepository.GetByIdAsync(request.Request.ShippingAddressId, cancellationToken);
            if (shippingAddress == null)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("آدرس ارسال یافت نشد");
            }
            else if (shippingAddress.UserId != request.UserId)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("دسترسی به این آدرس مجاز نیست");
            }

            // 3. Validate billing address if provided
            if (request.Request.BillingAddressId.HasValue)
            {
                var billingAddress = await _addressRepository.GetByIdAsync(request.Request.BillingAddressId.Value, cancellationToken);
                if (billingAddress == null || billingAddress.UserId != request.UserId)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add("آدرس صورتحساب نامعتبر است");
                }
            }

            // 4. Validate inventory
            decimal subtotal = 0;
            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add($"محصول {item.ProductId} یافت نشد");
                    continue;
                }

                if (!product.IsActive)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add($"محصول {product.Name} غیرفعال است");
                    continue;
                }

                var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId, cancellationToken);
                if (inventory == null)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add($"موجودی محصول {product.Name} یافت نشد");
                    continue;
                }

                var availableStock = inventory.GetAvailableStock();
                if (availableStock < item.Quantity)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add($"موجودی کافی برای محصول {product.Name} وجود ندارد. موجودی: {availableStock}، درخواستی: {item.Quantity}");
                    continue;
                }

                subtotal += item.TotalPrice;
            }

            // 5. Validate coupon if provided
            if (!string.IsNullOrEmpty(request.Request.CouponCode))
            {
                var coupon = await _couponRepository.GetByCodeAsync(request.Request.CouponCode, cancellationToken);
                if (coupon == null)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add("کد کوپن نامعتبر است");
                }
                else
                {
                    if (!coupon.IsActive)
                    {
                        validationResult.IsValid = false;
                        validationResult.Errors.Add("کوپن غیرفعال است");
                    }

                    if (coupon.EndDate < DateTime.UtcNow)
                    {
                        validationResult.IsValid = false;
                        validationResult.Errors.Add("کوپن منقضی شده است");
                    }

                    if (coupon.MinimumPurchase > 0 && subtotal < coupon.MinimumPurchase)
                    {
                        validationResult.IsValid = false;
                        validationResult.Errors.Add($"حداقل مبلغ سفارش برای این کوپن {coupon.MinimumPurchase} تومان است");
                    }
                }
            }

            return Result<CheckoutValidationResultDto>.Success(validationResult);
        }
    }
}

