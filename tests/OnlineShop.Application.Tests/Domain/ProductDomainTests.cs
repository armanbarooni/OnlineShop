using Xunit;

namespace OnlineShop.Application.Tests.Domain
{
    public class ProductDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesProduct()
        {
            // Act
            var product = OnlineShop.Domain.Entities.Product.Create(
                name: "Laptop",
                description: "Gaming Laptop",
                price: 15000000,
                stockQuantity: 10,
                mahakClientId: 123,
                mahakId: 456
            );

            // Assert
            Assert.NotNull(product);
            Assert.Equal("Laptop", product.Name);
            Assert.Equal("Gaming Laptop", product.Description);
            Assert.Equal(15000000, product.Price);
            Assert.Equal(10, product.StockQuantity);
            Assert.Equal(123, product.MahakClientId);
            Assert.Equal(456, product.MahakId);
            Assert.True(product.IsActive);
            Assert.False(product.Deleted);
            Assert.Equal(0, product.ViewCount);
        }

        [Fact]
        public void Create_EmptyName_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.Product.Create("", "Description", 1000, 10)
            );
            Assert.Contains("نام محصول", exception.Message);
        }

        [Fact]
        public void Create_NegativePrice_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.Product.Create("Product", "Description", -100, 10)
            );
            Assert.Contains("قیمت", exception.Message);
        }

        [Fact]
        public void Create_NegativeStockQuantity_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.Product.Create("Product", "Description", 1000, -5)
            );
            Assert.Contains("موجودی", exception.Message);
        }

        [Fact]
        public void SetName_ValidName_UpdatesName()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Old Name",
                "Description",
                1000,
                10
            );

            // Act
            product.SetName("New Name");

            // Assert
            Assert.Equal("New Name", product.Name);
        }

        [Fact]
        public void SetPrice_ValidPrice_UpdatesPrice()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );

            // Act
            product.SetPrice(1500);

            // Assert
            Assert.Equal(1500, product.Price);
        }

        [Fact]
        public void SetPrice_NegativePrice_ThrowsArgumentException()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                product.SetPrice(-100)
            );
            Assert.Contains("قیمت", exception.Message);
        }

        [Fact]
        public void Activate_InactiveProduct_ActivatesProduct()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );
            product.Deactivate();

            // Act
            product.Activate();

            // Assert
            Assert.True(product.IsActive);
        }

        [Fact]
        public void Deactivate_ActiveProduct_DeactivatesProduct()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );

            // Act
            product.Deactivate();

            // Assert
            Assert.False(product.IsActive);
        }

        [Fact]
        public void IncrementViewCount_IncreasesViewCountByOne()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );
            var initialViewCount = product.ViewCount;

            // Act
            product.IncrementViewCount();

            // Assert
            Assert.Equal(initialViewCount + 1, product.ViewCount);
        }

        [Fact]
        public void SetAsFeatured_SetsIsFeaturedToTrue()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );

            // Act
            product.SetAsFeatured();

            // Assert
            Assert.True(product.IsFeatured);
        }

        [Fact]
        public void RemoveFromFeatured_SetsIsFeaturedToFalse()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );
            product.SetAsFeatured();

            // Act
            product.RemoveFromFeatured();

            // Assert
            Assert.False(product.IsFeatured);
        }

        [Fact]
        public void Update_ValidData_UpdatesAllFields()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Old Product",
                "Old Description",
                1000,
                10
            );

            // Act
            product.Update(
                name: "New Product",
                description: "New Description",
                price: 2000,
                qty: 20,
                updatedBy: "admin123"
            );

            // Assert
            Assert.Equal("New Product", product.Name);
            Assert.Equal("New Description", product.Description);
            Assert.Equal(2000, product.Price);
            Assert.Equal(20, product.StockQuantity);
            Assert.Equal("admin123", product.UpdatedBy);
        }

        [Fact]
        public void Delete_NotDeleted_MarksAsDeleted()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );

            // Act
            product.Delete("admin123");

            // Assert
            Assert.True(product.Deleted);
            Assert.Equal("admin123", product.UpdatedBy);
        }

        [Fact]
        public void Delete_AlreadyDeleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var product = OnlineShop.Domain.Entities.Product.Create(
                "Product",
                "Description",
                1000,
                10
            );
            product.Delete("admin123");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                product.Delete("admin123")
            );
            Assert.Contains("قبلاً حذف شده", exception.Message);
        }
    }
}

