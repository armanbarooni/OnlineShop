using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Application.Features.Cart.Commands.AddToCart;
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

        private async Task<Guid> AddProductToCartAndGetCartIdAsync(Guid userId, Guid productId, int quantity)
        {
            var result = await AddToCartAsync(userId, productId, quantity);
            result.IsSuccess.Should().BeTrue(result.ErrorMessage);
            return result.Data!.Id;
        }

        private async Task<Guid> CreateProductWithInventoryAsync(int stockQuantity)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var product = Product.Create(
                $"Stock Contract Product {Guid.NewGuid():N}",
                "Inventory contract product",
                100_000,
                stockQuantity,
                mahakClientId: Random.Shared.Next(1000, 9999),
                mahakId: Random.Shared.Next(1000, 9999));

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
            int quantity)
        {
            using var scope = _factory.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.Send(new AddToCartCommand
            {
                UserId = userId,
                Item = new AddToCartDto
                {
                    ProductId = productId,
                    Quantity = quantity
                }
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
