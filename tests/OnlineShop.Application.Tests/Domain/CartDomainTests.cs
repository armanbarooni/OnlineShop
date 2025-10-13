using Xunit;

namespace OnlineShop.Application.Tests.Domain
{
    public class CartDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesCart()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var cart = OnlineShop.Domain.Entities.Cart.Create(
                userId,
                sessionId: "session123",
                cartName: "My Cart",
                expiresAt: DateTime.UtcNow.AddDays(7)
            );

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(userId, cart.UserId);
            Assert.Equal("session123", cart.SessionId);
            Assert.Equal("My Cart", cart.CartName);
            Assert.True(cart.IsActive);
            Assert.False(cart.Deleted);
        }

        [Fact]
        public void Activate_InactiveCart_ActivatesCart()
        {
            // Arrange
            var cart = OnlineShop.Domain.Entities.Cart.Create(
                Guid.NewGuid(),
                "session123"
            );
            cart.Deactivate();

            // Act
            cart.Activate();

            // Assert
            Assert.True(cart.IsActive);
        }

        [Fact]
        public void Deactivate_ActiveCart_DeactivatesCart()
        {
            // Arrange
            var cart = OnlineShop.Domain.Entities.Cart.Create(
                Guid.NewGuid(),
                "session123"
            );

            // Act
            cart.Deactivate();

            // Assert
            Assert.False(cart.IsActive);
        }

        [Fact]
        public void IsExpired_ExpiredCart_ReturnsTrue()
        {
            // Arrange
            var cart = OnlineShop.Domain.Entities.Cart.Create(
                Guid.NewGuid(),
                "session123",
                expiresAt: DateTime.UtcNow.AddDays(-1) // Expired yesterday
            );

            // Act
            var isExpired = cart.IsExpired();

            // Assert
            Assert.True(isExpired);
        }

        [Fact]
        public void IsExpired_NotExpiredCart_ReturnsFalse()
        {
            // Arrange
            var cart = OnlineShop.Domain.Entities.Cart.Create(
                Guid.NewGuid(),
                "session123",
                expiresAt: DateTime.UtcNow.AddDays(7)
            );

            // Act
            var isExpired = cart.IsExpired();

            // Assert
            Assert.False(isExpired);
        }

        [Fact]
        public void IsExpired_NoExpiryDate_ReturnsFalse()
        {
            // Arrange
            var cart = OnlineShop.Domain.Entities.Cart.Create(
                Guid.NewGuid(),
                "session123",
                expiresAt: null
            );

            // Act
            var isExpired = cart.IsExpired();

            // Assert
            Assert.False(isExpired);
        }

        [Fact]
        public void Delete_NotDeleted_MarksAsDeleted()
        {
            // Arrange
            var cart = OnlineShop.Domain.Entities.Cart.Create(
                Guid.NewGuid(),
                "session123"
            );

            // Act
            cart.Delete("user123");

            // Assert
            Assert.True(cart.Deleted);
            Assert.Equal("user123", cart.UpdatedBy);
        }
    }

    public class CartItemDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesCartItem()
        {
            // Arrange
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act
            var cartItem = OnlineShop.Domain.Entities.CartItem.Create(
                cartId,
                productId,
                quantity: 2,
                unitPrice: 1000,
                totalPrice: 2000
            );

            // Assert
            Assert.NotNull(cartItem);
            Assert.Equal(cartId, cartItem.CartId);
            Assert.Equal(productId, cartItem.ProductId);
            Assert.Equal(2, cartItem.Quantity);
            Assert.Equal(1000, cartItem.UnitPrice);
            Assert.Equal(2000, cartItem.TotalPrice);
            Assert.False(cartItem.Deleted);
        }

        [Fact]
        public void Update_ValidData_UpdatesCartItem()
        {
            // Arrange
            var cartItem = OnlineShop.Domain.Entities.CartItem.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                quantity: 2,
                unitPrice: 1000,
                totalPrice: 2000
            );

            // Act
            cartItem.Update(
                quantity: 5,
                unitPrice: 1200,
                notes: "Updated",
                updatedBy: "user123"
            );

            // Assert
            Assert.Equal(5, cartItem.Quantity);
            Assert.Equal(1200, cartItem.UnitPrice);
            Assert.Equal(6000, cartItem.TotalPrice); // 5 * 1200
            Assert.Equal("Updated", cartItem.Notes);
            Assert.Equal("user123", cartItem.UpdatedBy);
        }

        [Fact]
        public void SetQuantity_ValidQuantity_UpdatesQuantity()
        {
            // Arrange
            var cartItem = OnlineShop.Domain.Entities.CartItem.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                quantity: 2,
                unitPrice: 1000,
                totalPrice: 2000
            );

            // Act
            cartItem.SetQuantity(10);

            // Assert
            Assert.Equal(10, cartItem.Quantity);
        }

        [Fact]
        public void SetQuantity_ZeroOrNegative_ThrowsArgumentException()
        {
            // Arrange
            var cartItem = OnlineShop.Domain.Entities.CartItem.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                quantity: 2,
                unitPrice: 1000,
                totalPrice: 2000
            );

            // Act & Assert
            Assert.Throws<ArgumentException>(() => cartItem.SetQuantity(0));
            Assert.Throws<ArgumentException>(() => cartItem.SetQuantity(-5));
        }

        [Fact]
        public void Delete_NotDeleted_MarksAsDeleted()
        {
            // Arrange
            var cartItem = OnlineShop.Domain.Entities.CartItem.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                quantity: 2,
                unitPrice: 1000,
                totalPrice: 2000
            );

            // Act
            cartItem.Delete("user123");

            // Assert
            Assert.True(cartItem.Deleted);
            Assert.Equal("user123", cartItem.UpdatedBy);
        }
    }
}

