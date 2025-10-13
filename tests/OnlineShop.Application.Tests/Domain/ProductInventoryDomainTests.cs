using Xunit;

namespace OnlineShop.Application.Tests.Domain
{
    public class ProductInventoryDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesInventory()
        {
            // Arrange
            var productId = Guid.NewGuid();

            // Act
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                productId,
                availableQuantity: 100,
                reservedQuantity: 10,
                soldQuantity: 50,
                costPrice: 1000,
                sellingPrice: 1500
            );

            // Assert
            Assert.NotNull(inventory);
            Assert.Equal(productId, inventory.ProductId);
            Assert.Equal(100, inventory.AvailableQuantity);
            Assert.Equal(10, inventory.ReservedQuantity);
            Assert.Equal(50, inventory.SoldQuantity);
            Assert.Equal(1000, inventory.CostPrice);
            Assert.Equal(1500, inventory.SellingPrice);
            Assert.False(inventory.Deleted);
        }

        [Fact]
        public void GetAvailableStock_ReturnsCorrectValue()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100,
                reservedQuantity: 20
            );

            // Act
            var availableStock = inventory.GetAvailableStock();

            // Assert
            Assert.Equal(80, availableStock); // 100 - 20
        }

        [Fact]
        public void GetTotalQuantity_ReturnsCorrectValue()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100,
                reservedQuantity: 20,
                soldQuantity: 30
            );

            // Act
            var totalQuantity = inventory.GetTotalQuantity();

            // Assert
            Assert.Equal(150, totalQuantity); // 100 + 20 + 30
        }

        [Fact]
        public void ReserveQuantity_ValidQuantity_IncreasesReserved()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100,
                reservedQuantity: 10
            );
            var oldReserved = inventory.ReservedQuantity;

            // Act
            inventory.ReserveQuantity(20);

            // Assert
            Assert.Equal(30, inventory.ReservedQuantity); // 10 + 20
        }

        [Fact]
        public void ReserveQuantity_ExceedsAvailable_ThrowsInvalidOperationException()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100,
                reservedQuantity: 20
            );

            // Act & Assert - Available = 100 - 20 = 80, trying to reserve 90
            var exception = Assert.Throws<InvalidOperationException>(() =>
                inventory.ReserveQuantity(90)
            );
            Assert.Contains("موجودی کافی نیست", exception.Message);
        }

        [Fact]
        public void ReserveQuantity_ZeroOrNegative_ThrowsArgumentException()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100
            );

            // Act & Assert
            Assert.Throws<ArgumentException>(() => inventory.ReserveQuantity(0));
            Assert.Throws<ArgumentException>(() => inventory.ReserveQuantity(-10));
        }

        [Fact]
        public void ReleaseReservedQuantity_ValidQuantity_DecreasesReserved()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100,
                reservedQuantity: 50
            );

            // Act
            inventory.ReleaseReservedQuantity(20);

            // Assert
            Assert.Equal(30, inventory.ReservedQuantity); // 50 - 20
        }

        [Fact]
        public void ReleaseReservedQuantity_ExceedsReserved_ThrowsInvalidOperationException()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100,
                reservedQuantity: 20
            );

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                inventory.ReleaseReservedQuantity(30)
            );
            Assert.Contains("مقدار رزرو شده کافی نیست", exception.Message);
        }

        [Fact]
        public void CommitSale_ValidQuantity_MovesFromReservedToSold()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100,
                reservedQuantity: 50,
                soldQuantity: 30
            );

            // Act
            inventory.CommitSale(20);

            // Assert
            Assert.Equal(30, inventory.ReservedQuantity); // 50 - 20
            Assert.Equal(50, inventory.SoldQuantity); // 30 + 20
        }

        [Fact]
        public void CommitSale_ExceedsReserved_ThrowsInvalidOperationException()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100,
                reservedQuantity: 20
            );

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                inventory.CommitSale(30)
            );
            Assert.Contains("مقدار رزرو شده کافی نیست", exception.Message);
        }

        [Fact]
        public void AddStock_ValidQuantity_IncreasesAvailable()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100
            );

            // Act
            inventory.AddStock(50);

            // Assert
            Assert.Equal(150, inventory.AvailableQuantity);
        }

        [Fact]
        public void AddStock_ZeroOrNegative_ThrowsArgumentException()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100
            );

            // Act & Assert
            Assert.Throws<ArgumentException>(() => inventory.AddStock(0));
            Assert.Throws<ArgumentException>(() => inventory.AddStock(-10));
        }

        [Fact]
        public void SetAvailableQuantity_NegativeValue_ThrowsArgumentException()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100
            );

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                inventory.SetAvailableQuantity(-10)
            );
            Assert.Contains("تعداد موجودی نمی‌تواند منفی باشد", exception.Message);
        }

        [Fact]
        public void Delete_NotDeleted_MarksAsDeleted()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100
            );

            // Act
            inventory.Delete("user123");

            // Assert
            Assert.True(inventory.Deleted);
            Assert.Equal("user123", inventory.UpdatedBy);
        }

        [Fact]
        public void Delete_AlreadyDeleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var inventory = OnlineShop.Domain.Entities.ProductInventory.Create(
                Guid.NewGuid(),
                availableQuantity: 100
            );
            inventory.Delete("user123");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                inventory.Delete("user123")
            );
            Assert.Contains("قبلاً حذف شده است", exception.Message);
        }
    }
}

