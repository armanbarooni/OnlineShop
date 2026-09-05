using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Application.Features.Cart.Commands.AddToCart;
using OnlineShop.Application.Features.Cart.Commands.RemoveFromCart;
using OnlineShop.Application.Features.Cart.Queries.GetCart;
using OnlineShop.Application.Features.Checkout.Commands.ProcessCheckout;
using OnlineShop.Application.Features.Checkout.Commands.ValidateCheckout;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.IntegrationTests.Infrastructure;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class CartInventoryContractTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public CartInventoryContractTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task E1_AddToCart_WithEnoughStock_ShouldAddItem()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 2);

            var result = await AddToCartAsync(userId, productId, quantity: 1);

            result.IsSuccess.Should().BeTrue(result.ErrorMessage);
            result.Data!.Items.Should().Contain(i => i.ProductId == productId && i.Quantity == 1);
        }

        [Fact]
        public async Task E2_AddToCart_WhenProductHasOneStockAndTwoRequested_ShouldNotAddItem()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 1);

            var result = await AddToCartAsync(userId, productId, quantity: 2);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("موجودی");
        }

        [Fact]
        public async Task E3_AddToCart_ShouldCheckDatabaseStockBeforeAdding()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 2);
            await SetProductStockAsync(productId, stockQuantity: 1);

            var result = await AddToCartAsync(userId, productId, quantity: 2);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("موجودی");
        }

        [Fact]
        public async Task E4_CheckoutValidation_ShouldRecheckInventoryBeforeCheckout()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 2);
            var cartId = await AddProductToCartAndGetCartIdAsync(userId, productId, quantity: 2);
            var addressId = await CreateAddressAsync(userId);

            var result = await ValidateCheckoutAsync(userId, cartId, addressId);

            result.IsSuccess.Should().BeTrue(result.ErrorMessage);
            result.Data!.IsValid.Should().BeTrue(string.Join(", ", result.Data.Errors));
        }

        [Fact]
        public async Task E5_Checkout_WhenStockBecameInsufficient_ShouldStopCheckoutAndReturnStockMessage()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 2);
            var cartId = await AddProductToCartAndGetCartIdAsync(userId, productId, quantity: 2);
            var addressId = await CreateAddressAsync(userId);
            await SetProductStockAsync(productId, stockQuantity: 1);

            var result = await ProcessCheckoutAsync(userId, cartId, addressId);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("موجودی");
        }

        [Fact]
        public async Task E6_ProductWithPrice2_ShouldUseDiscountedPriceInCartAndCheckout()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(
                stockQuantity: 2,
                price: 100_000m,
                price2: 70_000m);
            var cartId = await AddProductToCartAndGetCartIdAsync(userId, productId, quantity: 1);
            var addressId = await CreateAddressAsync(userId);

            using (var scope = _factory.Services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var cartResult = await mediator.Send(new GetCartQuery { UserId = userId });

                cartResult.IsSuccess.Should().BeTrue(cartResult.ErrorMessage);
                var cartItem = cartResult.Data!.Items.Single(i => i.ProductId == productId);
                cartItem.OriginalUnitPrice.Should().Be(100_000m);
                cartItem.UnitPrice.Should().Be(70_000m);
                cartItem.TotalPrice.Should().Be(70_000m);
                cartItem.HasDiscount.Should().BeTrue();
                cartResult.Data.Subtotal.Should().Be(70_000m);
            }

            var checkoutResult = await ProcessCheckoutAsync(userId, cartId, addressId);

            checkoutResult.IsSuccess.Should().BeTrue(checkoutResult.ErrorMessage);
            checkoutResult.Data!.Summary.SubTotal.Should().Be(70_000m);
            checkoutResult.Data.Summary.TotalAmount.Should().Be(70_000m);
            checkoutResult.Data.Summary.Items.Single().UnitPrice.Should().Be(70_000m);

            using var verificationScope = _factory.Services.CreateScope();
            var db = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedOrder = await db.UserOrders
                .Include(order => order.OrderItems)
                .SingleAsync(order => order.Id == checkoutResult.Data.Order.Id);
            savedOrder.TotalAmount.Should().Be(70_000m);
            savedOrder.OrderItems.Single().UnitPrice.Should().Be(70_000m);
        }

        [Fact]
        public async Task E7_RemovedCartItem_ShouldBeRestoredWhenAddedAgain()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 2);

            var firstAdd = await AddToCartAsync(userId, productId, quantity: 1);
            firstAdd.IsSuccess.Should().BeTrue(firstAdd.ErrorMessage);
            var originalItemId = firstAdd.Data!.Items.Single(i => i.ProductId == productId).Id;

            var removeResult = await RemoveFromCartAsync(userId, originalItemId);
            removeResult.IsSuccess.Should().BeTrue(removeResult.ErrorMessage);

            var secondAdd = await AddToCartAsync(userId, productId, quantity: 1);
            secondAdd.IsSuccess.Should().BeTrue(secondAdd.ErrorMessage);
            secondAdd.Data!.Items.Single(i => i.ProductId == productId).Id.Should().Be(originalItemId);

            using var verificationScope = _factory.Services.CreateScope();
            var db = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedItems = await db.CartItems
                .IgnoreQueryFilters()
                .Where(item => item.CartId == firstAdd.Data.Id && item.ProductId == productId)
                .ToListAsync();

            storedItems.Should().ContainSingle();
            storedItems.Single().Deleted.Should().BeFalse();
        }

        [Fact]
        public async Task E8_Checkout_ShouldRefreshStaleInventoryFromProductStock()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 2);
            var cartId = await AddProductToCartAndGetCartIdAsync(userId, productId, quantity: 1);
            var addressId = await CreateAddressAsync(userId);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var inventory = await db.ProductInventories.SingleAsync(i => i.ProductId == productId);
                inventory.SetAvailableQuantity(0);
                await db.SaveChangesAsync();
            }

            var checkoutResult = await ProcessCheckoutAsync(userId, cartId, addressId);

            checkoutResult.IsSuccess.Should().BeTrue(checkoutResult.ErrorMessage);
            checkoutResult.Data!.Order.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task E9_Checkout_ShouldDiscardOrphanedInventoryReservation()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 1);
            var cartId = await AddProductToCartAndGetCartIdAsync(userId, productId, quantity: 1);
            var addressId = await CreateAddressAsync(userId);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var inventory = await db.ProductInventories.SingleAsync(i => i.ProductId == productId);
                inventory.SetReservedQuantity(1);
                await db.SaveChangesAsync();
            }

            var checkoutResult = await ProcessCheckoutAsync(userId, cartId, addressId);

            checkoutResult.IsSuccess.Should().BeTrue(checkoutResult.ErrorMessage);
            checkoutResult.Data!.Order.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task E10_VariantReservation_ShouldLockAndReleaseExactSizeAndColor()
        {
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 1);
            Guid variantId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var variant = ProductVariant.Create(productId, "2XL", "سبز", $"V-{Guid.NewGuid():N}", 1);
                await db.ProductVariants.AddAsync(variant);
                await db.SaveChangesAsync();
                variantId = variant.Id;
            }

            Guid orderId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var order = UserOrder.Create(Guid.NewGuid(), $"LOCK-{Guid.NewGuid():N}", 100, 0, 0, 0, 100);
                await db.UserOrders.AddAsync(order);
                await db.SaveChangesAsync();
                await db.UserOrderItems.AddAsync(UserOrderItem.Create(order.Id, productId, variantId, "Locked variant", 1, 100, 100));
                await db.SaveChangesAsync();
                orderId = order.Id;
            }

            using (var scope = _factory.Services.CreateScope())
            {
                var inventory = scope.ServiceProvider.GetRequiredService<OnlineShop.Application.Services.IInventoryService>();
                await inventory.ReserveStockForOrder(orderId, new() { (productId, variantId, 1) }, default);
            }

            using (var scope = _factory.Services.CreateScope())
            {
                var inventory = scope.ServiceProvider.GetRequiredService<OnlineShop.Application.Services.IInventoryService>();
                var reserveAgain = () => inventory.ReserveStockForOrder(
                    Guid.Empty, new() { (productId, variantId, 1) }, default);
                await reserveAgain.Should().ThrowAsync<InvalidOperationException>();

                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var order = await db.UserOrders.SingleAsync(o => o.Id == orderId);
                order.Cancel("test release");
                await db.SaveChangesAsync();
                var repository = scope.ServiceProvider.GetRequiredService<OnlineShop.Domain.Interfaces.Repositories.IProductInventoryRepository>();
                await repository.ReleaseReservationsAsync(new[] { (productId, (Guid?)variantId, 1) }, default);
            }

            using (var scope = _factory.Services.CreateScope())
            {
                var inventory = scope.ServiceProvider.GetRequiredService<OnlineShop.Application.Services.IInventoryService>();
                await inventory.ReserveStockForOrder(Guid.Empty, new() { (productId, variantId, 1) }, default);
            }
        }

        [Fact]
        public async Task E11_ProductDto_ShouldExposeAvailableVariantStockOnly()
        {
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 1);
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var variant = ProductVariant.Create(productId, "XL", "مشکی", $"DTO-{Guid.NewGuid():N}", 1);
            variant.ReconcileReservedQuantity(1);
            await db.ProductVariants.AddAsync(variant);
            await db.SaveChangesAsync();

            var repository = scope.ServiceProvider.GetRequiredService<OnlineShop.Domain.Interfaces.Repositories.IProductRepository>();
            var mapper = scope.ServiceProvider.GetRequiredService<AutoMapper.IMapper>();
            var product = await repository.GetByIdWithIncludesAsync(productId, default);
            var dto = mapper.Map<OnlineShop.Application.DTOs.Product.ProductDto>(product);

            dto.StockQuantity.Should().Be(0);
            dto.Variants.Single(v => v.Id == variant.Id).StockQuantity.Should().Be(0);
        }

        [Fact]
        public async Task E12_Cart_ShouldExposeSelectedVariantSizeAndColor()
        {
            var userId = Guid.NewGuid();
            var productId = await CreateProductWithInventoryAsync(stockQuantity: 2);
            Guid variantId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var variant = ProductVariant.Create(productId, "2XL", "سبز", $"CART-{Guid.NewGuid():N}", 2);
                await db.ProductVariants.AddAsync(variant);
                await db.SaveChangesAsync();
                variantId = variant.Id;
            }

            var addResult = await AddToCartAsync(userId, productId, quantity: 1, variantId: variantId);
            addResult.IsSuccess.Should().BeTrue(addResult.ErrorMessage);
            addResult.Data!.Items.Single().VariantInfo.Should().Be("سایز: 2XL، رنگ: سبز");

            using var verificationScope = _factory.Services.CreateScope();
            var mediator = verificationScope.ServiceProvider.GetRequiredService<IMediator>();
            var cartResult = await mediator.Send(new GetCartQuery { UserId = userId });

            cartResult.IsSuccess.Should().BeTrue(cartResult.ErrorMessage);
            cartResult.Data!.Items.Single().VariantInfo.Should().Be("سایز: 2XL، رنگ: سبز");
        }

        private async Task<Guid> AddProductToCartAndGetCartIdAsync(Guid userId, Guid productId, int quantity)
        {
            var result = await AddToCartAsync(userId, productId, quantity);
            result.IsSuccess.Should().BeTrue(result.ErrorMessage);
            return result.Data!.Id;
        }

        private async Task<Guid> CreateProductWithInventoryAsync(
            int stockQuantity,
            decimal price = 100_000m,
            decimal? price2 = null)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var product = Product.Create(
                $"Stock Contract Product {Guid.NewGuid():N}",
                "Inventory contract product",
                price,
                stockQuantity,
                mahakClientId: Random.Shared.Next(1000, 9999),
                mahakId: Random.Shared.Next(1000, 9999));

            product.SetPrice2(price2);

            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();

            await db.ProductInventories.AddAsync(ProductInventory.Create(product.Id, stockQuantity));
            await db.SaveChangesAsync();

            return product.Id;
        }

        private async Task SetProductStockAsync(Guid productId, int stockQuantity)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var product = await db.Products.SingleAsync(p => p.Id == productId);
            product.SetStockQuantity(stockQuantity);

            var inventory = await db.ProductInventories.SingleAsync(i => i.ProductId == productId);
            inventory.SetAvailableQuantity(stockQuantity);
            inventory.SetReservedQuantity(0);

            await db.SaveChangesAsync();
        }

        private async Task<Guid> CreateAddressAsync(Guid userId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var address = UserAddress.Create(
                userId,
                "Checkout Address",
                "Contract",
                "User",
                "Test address line",
                "Tehran",
                "Tehran",
                "1234567890",
                "Iran",
                null,
                "09123456789");

            address.SetAsShippingAddress(true);
            address.SetAsBillingAddress(true);

            await db.UserAddresses.AddAsync(address);
            await db.SaveChangesAsync();

            return address.Id;
        }

        private async Task<OnlineShop.Application.Common.Models.Result<OnlineShop.Application.DTOs.Cart.CartDto>> AddToCartAsync(
            Guid userId,
            Guid productId,
            int quantity,
            Guid? variantId = null)
        {
            using var scope = _factory.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(new AddToCartCommand
            {
                UserId = userId,
                Item = new AddToCartDto
                {
                    ProductId = productId,
                    VariantId = variantId ?? Guid.Empty,
                    Quantity = quantity
                }
            });
        }

        private async Task<OnlineShop.Application.Common.Models.Result<OnlineShop.Application.DTOs.Cart.CartDto>> RemoveFromCartAsync(
            Guid userId,
            Guid cartItemId)
        {
            using var scope = _factory.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(new RemoveFromCartCommand
            {
                UserId = userId,
                CartItemId = cartItemId
            });
        }

        private async Task<OnlineShop.Application.Common.Models.Result<CheckoutValidationResultDto>> ValidateCheckoutAsync(
            Guid userId,
            Guid cartId,
            Guid addressId)
        {
            using var scope = _factory.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(new ValidateCheckoutCommand
            {
                UserId = userId,
                Request = new CheckoutRequestDto
                {
                    CartId = cartId,
                    ShippingAddressId = addressId
                }
            });
        }

        private async Task<OnlineShop.Application.Common.Models.Result<CheckoutResultDto>> ProcessCheckoutAsync(
            Guid userId,
            Guid cartId,
            Guid addressId)
        {
            using var scope = _factory.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(new ProcessCheckoutCommand
            {
                UserId = userId,
                Request = new CheckoutRequestDto
                {
                    CartId = cartId,
                    ShippingAddressId = addressId
                }
            });
        }
    }
}
