using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.Services;

namespace OnlineShop.Application.Features.Checkout.Commands.ProcessCheckout
{
    public class ProcessCheckoutCommandHandler : IRequestHandler<ProcessCheckoutCommand, Result<CheckoutResultDto>>
    {
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductInventoryRepository _inventoryRepository;
    private readonly IInventoryService _inventoryService;
        private readonly IUserOrderRepository _orderRepository;
        private readonly IUserOrderItemRepository _orderItemRepository;
        private readonly IUserAddressRepository _addressRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IUserCouponUsageRepository _userCouponUsageRepository;
        private readonly IMapper _mapper;

        public ProcessCheckoutCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IProductInventoryRepository inventoryRepository,
            IInventoryService inventoryService,
            IUserOrderRepository orderRepository,
            IUserOrderItemRepository orderItemRepository,
            IUserAddressRepository addressRepository,
            ICouponRepository couponRepository,
            IUserCouponUsageRepository userCouponUsageRepository,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _addressRepository = addressRepository;
            _couponRepository = couponRepository;
            _userCouponUsageRepository = userCouponUsageRepository;
            _mapper = mapper;
        }

        public async Task<Result<CheckoutResultDto>> Handle(ProcessCheckoutCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate cart exists and belongs to user
            var cart = await _cartRepository.GetByIdAsync(request.Request.CartId, cancellationToken);
            if (cart == null)
                return Result<CheckoutResultDto>.Failure("سبد خرید یافت نشد");

            if (cart.UserId != request.UserId)
                return Result<CheckoutResultDto>.Failure("دسترسی به این سبد خرید مجاز نیست");

            var cartItems = await _cartRepository.GetCartItemsAsync(cart.Id, cancellationToken);
            if (!cartItems.Any())
                return Result<CheckoutResultDto>.Failure("سبد خرید خالی است");

            // 2. Validate shipping address exists and belongs to user
            var shippingAddress = await _addressRepository.GetByIdAsync(request.Request.ShippingAddressId, cancellationToken);
            if (shippingAddress == null)
                return Result<CheckoutResultDto>.Failure("آدرس ارسال یافت نشد");

            if (shippingAddress.UserId != request.UserId)
                return Result<CheckoutResultDto>.Failure("دسترسی به این آدرس مجاز نیست");

            // 3. Validate billing address if provided
            Guid? billingAddressId = request.Request.BillingAddressId;
            if (billingAddressId.HasValue)
            {
                var billingAddress = await _addressRepository.GetByIdAsync(billingAddressId.Value, cancellationToken);
                if (billingAddress == null || billingAddress.UserId != request.UserId)
                    return Result<CheckoutResultDto>.Failure("آدرس صورتحساب نامعتبر است");
            }
            else
            {
                // Default to shipping address
                billingAddressId = request.Request.ShippingAddressId;
            }

            // 4. Validate inventory and reserve stock (use InventoryService for atomic multi-item reservation)
            decimal subtotal = 0;
            var reservationItems = new List<(Guid ProductId, int Quantity)>();

            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null)
                    return Result<CheckoutResultDto>.Failure($"محصول {item.ProductId} یافت نشد");

                if (!product.IsActive)
                    return Result<CheckoutResultDto>.Failure($"محصول {product.Name} غیرفعال است");

                reservationItems.Add((item.ProductId, item.Quantity));
                subtotal += item.TotalPrice;
            }

            // Attempt atomic reservation for all items in the cart
            try
            {
                await _inventoryService.ReserveStockForOrder(Guid.Empty, reservationItems, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return Result<CheckoutResultDto>.Failure(ex.Message);
            }

            // 5. Validate and apply coupon if provided
            decimal discountAmount = request.Request.DiscountAmount;
            Guid? appliedCouponId = null;
            
            if (!string.IsNullOrEmpty(request.Request.CouponCode))
            {
                var coupon = await _couponRepository.GetByCodeAsync(request.Request.CouponCode, cancellationToken);
                if (coupon == null)
                    return Result<CheckoutResultDto>.Failure("کد کوپن نامعتبر است");

                // Validate coupon
                if (!coupon.IsActive)
                    return Result<CheckoutResultDto>.Failure("کوپن غیرفعال است");

                if (coupon.EndDate < DateTime.UtcNow)
                    return Result<CheckoutResultDto>.Failure("کوپن منقضی شده است");

                if (coupon.UsageLimit > 0)
                {
                    var usageCount = await _userCouponUsageRepository.GetUsageCountByUserAsync(request.UserId.ToString(), coupon.Id, cancellationToken);
                    if (usageCount >= coupon.UsageLimit)
                        return Result<CheckoutResultDto>.Failure("حد مجاز استفاده از این کوپن تمام شده است");
                }

                if (coupon.MinimumPurchase > 0 && subtotal < coupon.MinimumPurchase)
                    return Result<CheckoutResultDto>.Failure($"حداقل مبلغ سفارش برای این کوپن {coupon.MinimumPurchase} تومان است");

                // Calculate discount using the entity method
                discountAmount = coupon.CalculateDiscount(subtotal);

                // Ensure discount doesn't exceed subtotal
                discountAmount = Math.Min(discountAmount, subtotal);
                appliedCouponId = coupon.Id;
            }

            // 6. Calculate totals
            var taxAmount = subtotal * request.Request.TaxRate;
            var totalAmount = subtotal + taxAmount + request.Request.ShippingCost - discountAmount;

            // 7. Generate order number
            var orderNumber = await _orderRepository.GenerateOrderNumberAsync(cancellationToken);

            // 8. Create order
            var order = Domain.Entities.UserOrder.Create(
                request.UserId,
                orderNumber,
                subtotal,
                taxAmount,
                request.Request.ShippingCost,
                discountAmount,
                totalAmount,
                "IRR"
            );

            order.SetShippingAddress(request.Request.ShippingAddressId);
            order.SetBillingAddress(billingAddressId.Value);
            order.SetNotes(request.Request.Notes);

            await _orderRepository.AddAsync(order, cancellationToken);

            // 9. Record coupon usage if applied
            if (appliedCouponId.HasValue)
            {
                var couponUsage = Domain.Entities.UserCouponUsage.Create(
                    request.UserId.ToString(),
                    appliedCouponId.Value,
                    order.Id,
                    discountAmount,
                    totalAmount
                );
                await _userCouponUsageRepository.AddAsync(couponUsage, cancellationToken);
            }

            // 10. Create order items
            var orderItemSummaries = new List<OrderItemSummaryDto>();
            foreach (var cartItem in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
                var productName = product?.Name ?? "Unknown Product";
                
                var orderItem = Domain.Entities.UserOrderItem.Create(
                    order.Id,
                    cartItem.ProductId,
                    productName,
                    cartItem.Quantity,
                    cartItem.UnitPrice,
                    cartItem.TotalPrice
                );

                await _orderItemRepository.AddAsync(orderItem, cancellationToken);

                orderItemSummaries.Add(new OrderItemSummaryDto
                {
                    ProductId = cartItem.ProductId,
                    ProductName = productName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.TotalPrice
                });
            }

            // 11. Inventory was already reserved atomically by InventoryService

            // 12. Clear cart
            await _cartRepository.ClearCartAsync(cart.Id, cancellationToken);

            // 13. Prepare result
            var orderDto = _mapper.Map<OnlineShop.Application.DTOs.UserOrder.UserOrderDto>(order);
            
            var summary = new OrderSummaryDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TotalItems = cartItems.Count(),
                SubTotal = subtotal,
                TaxAmount = taxAmount,
                ShippingAmount = request.Request.ShippingCost,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Currency = "IRR",
                OrderDate = order.CreatedAt,
                Items = orderItemSummaries
            };

            var result = new CheckoutResultDto
            {
                Order = orderDto,
                Summary = summary,
                Message = "سفارش شما با موفقیت ثبت شد و موجودی محصولات رزرو گردید"
            };

            return Result<CheckoutResultDto>.Success(result);
        }
    }
}


