using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Checkout.Commands.ProcessCheckout
{
    public class ProcessCheckoutCommandHandler : IRequestHandler<ProcessCheckoutCommand, Result<CheckoutResultDto>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductInventoryRepository _inventoryRepository;
        private readonly IUserOrderRepository _orderRepository;
        private readonly IUserOrderItemRepository _orderItemRepository;
        private readonly IUserAddressRepository _addressRepository;
        private readonly IMapper _mapper;

        public ProcessCheckoutCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IProductInventoryRepository inventoryRepository,
            IUserOrderRepository orderRepository,
            IUserOrderItemRepository orderItemRepository,
            IUserAddressRepository addressRepository,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _addressRepository = addressRepository;
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

            // 4. Validate inventory and reserve stock
            decimal subtotal = 0;
            var inventoryUpdates = new List<(Domain.Entities.ProductInventory inventory, int quantity)>();

            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null)
                    return Result<CheckoutResultDto>.Failure($"محصول {item.ProductId} یافت نشد");

                if (!product.IsActive)
                    return Result<CheckoutResultDto>.Failure($"محصول {product.Name} غیرفعال است");

                var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId, cancellationToken);
                if (inventory == null)
                    return Result<CheckoutResultDto>.Failure($"موجودی محصول {product.Name} یافت نشد");

                var availableStock = inventory.GetAvailableStock();
                if (availableStock < item.Quantity)
                    return Result<CheckoutResultDto>.Failure($"موجودی کافی برای محصول {product.Name} وجود ندارد. موجودی: {availableStock}، درخواستی: {item.Quantity}");

                // Reserve inventory
                try
                {
                    inventory.ReserveQuantity(item.Quantity);
                    inventoryUpdates.Add((inventory, item.Quantity));
                }
                catch (Exception ex)
                {
                    return Result<CheckoutResultDto>.Failure($"خطا در رزرو موجودی: {ex.Message}");
                }

                subtotal += item.TotalPrice;
            }

            // 5. Calculate totals
            var taxAmount = subtotal * request.Request.TaxRate;
            var totalAmount = subtotal + taxAmount + request.Request.ShippingCost - request.Request.DiscountAmount;

            // 6. Generate order number
            var orderNumber = await _orderRepository.GenerateOrderNumberAsync(cancellationToken);

            // 7. Create order
            var order = Domain.Entities.UserOrder.Create(
                request.UserId,
                orderNumber,
                subtotal,
                taxAmount,
                request.Request.ShippingCost,
                request.Request.DiscountAmount,
                totalAmount,
                "IRR"
            );

            order.SetShippingAddress(request.Request.ShippingAddressId);
            order.SetBillingAddress(billingAddressId.Value);
            order.SetNotes(request.Request.Notes);

            await _orderRepository.AddAsync(order, cancellationToken);

            // 8. Create order items
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

            // 9. Update inventory
            foreach (var (inventory, quantity) in inventoryUpdates)
            {
                await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
            }

            // 10. Clear cart
            await _cartRepository.ClearCartAsync(cart.Id, cancellationToken);

            // 11. Prepare result
            var orderDto = _mapper.Map<OnlineShop.Application.DTOs.UserOrder.UserOrderDto>(order);
            
            var summary = new OrderSummaryDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TotalItems = cartItems.Count(),
                SubTotal = subtotal,
                TaxAmount = taxAmount,
                ShippingAmount = request.Request.ShippingCost,
                DiscountAmount = request.Request.DiscountAmount,
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


