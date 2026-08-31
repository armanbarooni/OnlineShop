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
    private readonly IProductVariantRepository _productVariantRepository;
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
            IProductVariantRepository productVariantRepository,
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
            _productVariantRepository = productVariantRepository;
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
            var reservationItems = new List<(Guid ProductId, Guid? VariantId, int Quantity)>();
            var productPrices = new Dictionary<Guid, decimal>();

            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null)
                    return Result<CheckoutResultDto>.Failure($"محصول {item.ProductId} یافت نشد");

                if (!product.IsActive)
                    return Result<CheckoutResultDto>.Failure($"محصول {product.Name} غیرفعال است");

                if (item.VariantId is { } variantId && variantId != Guid.Empty)
                {
                    var variant = await _productVariantRepository.GetByIdAsync(variantId, cancellationToken);
                    if (variant == null || variant.ProductId != item.ProductId)
                        return Result<CheckoutResultDto>.Failure($"رنگ یا سایز انتخاب‌شده برای {product.Name} معتبر نیست");

                    if (!variant.IsAvailable || variant.GetAvailableStock() < item.Quantity)
                        return Result<CheckoutResultDto>.Failure($"موجودی رنگ و سایز انتخاب‌شده برای {product.Name} کافی نیست");
                }

                var unitPrice = product.GetCurrentPrice();
                productPrices[item.ProductId] = unitPrice;
                reservationItems.Add((item.ProductId, item.VariantId, item.Quantity));
                subtotal += unitPrice * item.Quantity;
            }

            // 5. Simplified logic: No coupons, fixed costs
            decimal discountAmount = 0m;
            decimal shippingCost = request.Request.ShippingCost ?? 0m;
            decimal taxAmount = 0m; // Set to 0 or desired default
            decimal totalAmount = subtotal + taxAmount + shippingCost - discountAmount;

            // 7. Generate order number
            var orderNumber = await _orderRepository.GenerateOrderNumberAsync(cancellationToken);

            // 8. Create order
            var order = Domain.Entities.UserOrder.Create(
                request.UserId,
                orderNumber,
                subtotal,
                taxAmount,
                shippingCost,
                discountAmount,
                totalAmount,
                "IRR"
            );

            order.SetShippingAddress(request.Request.ShippingAddressId);
            order.SetBillingAddress(billingAddressId.Value);
            order.SetNotes(request.Request.Notes);

            await _orderRepository.AddAsync(order, cancellationToken);

            // 10. Create order items
            var orderItemSummaries = new List<OrderItemSummaryDto>();
            foreach (var cartItem in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
                var productName = product?.Name ?? "Unknown Product";
                var unitPrice = productPrices.TryGetValue(cartItem.ProductId, out var currentPrice)
                    ? currentPrice
                    : cartItem.UnitPrice;
                var totalPrice = unitPrice * cartItem.Quantity;
                
                var orderItem = Domain.Entities.UserOrderItem.Create(
                    order.Id,
                    cartItem.ProductId,
                    cartItem.VariantId,
                    productName,
                    cartItem.Quantity,
                    unitPrice,
                    totalPrice
                );

                var regularUnitPrice = product?.Price ?? unitPrice;
                var productDiscount = Math.Max(0m, regularUnitPrice - unitPrice) * cartItem.Quantity;
                if (productDiscount > 0)
                {
                    orderItem.SetDiscountAmount(productDiscount);
                }

                await _orderItemRepository.AddAsync(orderItem, cancellationToken);

                orderItemSummaries.Add(new OrderItemSummaryDto
                {
                    ProductId = cartItem.ProductId,
                    ProductName = productName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            // The pending order exists before reservation, so every lock has an owner and
            // can be committed by the payment callback or released by the timeout worker.
            try
            {
                await _inventoryService.ReserveStockForOrder(order.Id, reservationItems, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                order.Cancel(ex.Message);
                await _orderRepository.UpdateAsync(order, cancellationToken);
                return Result<CheckoutResultDto>.Failure(ex.Message);
            }

            // 11. Inventory was already reserved atomically by InventoryService

            // 13. Prepare result
            var orderDto = _mapper.Map<OnlineShop.Application.DTOs.UserOrder.UserOrderDto>(order);
            
            var summary = new OrderSummaryDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TotalItems = cartItems.Count(),
                SubTotal = subtotal,
                TaxAmount = taxAmount,
                ShippingAmount = shippingCost,
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

            // 12. Clear cart (moved here to prevent clearing cart when command fails or throws exception)
            await _cartRepository.ClearCartAsync(cart.Id, cancellationToken);

            return Result<CheckoutResultDto>.Success(result);
        }
    }
}


