using Xunit;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Domain
{
    public class ProductCategoryDomainTests
    {
        [Fact]
        public void Create_ValidData_CreatesCategory()
        {
            // Act
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Electronics",
                "Electronic products",
                123,
                456
            );

            // Assert
            Assert.NotNull(category);
            Assert.Equal("Electronics", category.Name);
            Assert.Equal("Electronic products", category.Description);
            Assert.Equal(123, category.MahakClientId);
            Assert.Equal(456, category.MahakId);
            Assert.False(category.Deleted);
        }

        [Fact]
        public void Create_EmptyName_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.ProductCategory.Create("", "Description", 123, 456));

            Assert.Contains("Category name cannot be empty", exception.Message);
        }

        [Fact]
        public void Create_WhitespaceName_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.ProductCategory.Create("   ", "Description", 123, 456));

            Assert.Contains("Category name cannot be empty", exception.Message);
        }

        [Fact]
        public void Create_NullName_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                OnlineShop.Domain.Entities.ProductCategory.Create(null!, "Description", 123, 456));

            Assert.Contains("Category name cannot be empty", exception.Message);
        }

        [Fact]
        public void SetName_ValidName_UpdatesName()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Old Name",
                "Description",
                123,
                456
            );
            var oldUpdatedAt = category.UpdatedAt;
            Thread.Sleep(10); // تضمین تفاوت زمانی

            // Act
            category.SetName("New Name");

            // Assert
            Assert.Equal("New Name", category.Name);
            Assert.True(category.UpdatedAt > oldUpdatedAt);
        }

        [Fact]
        public void SetName_NameWithSpaces_TrimsName()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Old Name",
                "Description",
                123,
                456
            );

            // Act
            category.SetName("  Trimmed Name  ");

            // Assert
            Assert.Equal("Trimmed Name", category.Name);
        }

        [Fact]
        public void SetName_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Original Name",
                "Description",
                123,
                456
            );

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => category.SetName(""));
            Assert.Contains("Category name cannot be empty", exception.Message);
        }

        [Fact]
        public void SetDescription_ValidDescription_UpdatesDescription()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Name",
                "Old Description",
                123,
                456
            );

            // Act
            category.SetDescription("New Description");

            // Assert
            Assert.Equal("New Description", category.Description);
        }

        [Fact]
        public void Update_ValidData_UpdatesAllFields()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Old Name",
                "Old Description",
                123,
                456
            );

            // Act
            category.Update("New Name", "New Description", "user123");

            // Assert
            Assert.Equal("New Name", category.Name);
            Assert.Equal("New Description", category.Description);
            Assert.Equal("user123", category.UpdatedBy);
        }

        [Fact]
        public void Update_NullUpdatedBy_UsesDefaultValue()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Old Name",
                "Old Description",
                123,
                456
            );

            // Act
            category.Update("New Name", "New Description", null);

            // Assert
            Assert.Null(category.UpdatedBy);
        }

        [Fact]
        public void Delete_NotDeletedCategory_MarksAsDeleted()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Name",
                "Description",
                123,
                456
            );

            // Act
            category.Delete("user123");

            // Assert
            Assert.True(category.Deleted);
            Assert.Equal("user123", category.UpdatedBy);
        }

        [Fact]
        public void Delete_AlreadyDeleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Name",
                "Description",
                123,
                456
            );
            category.Delete("user123");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => category.Delete("user123"));
            Assert.Contains("already deleted", exception.Message);
        }

        [Fact]
        public void Delete_NullUpdatedBy_UsesDefaultValue()
        {
            // Arrange
            var category = OnlineShop.Domain.Entities.ProductCategory.Create(
                "Name",
                "Description",
                123,
                456
            );

            // Act
            category.Delete(null);

            // Assert
            Assert.True(category.Deleted);
            Assert.Null(category.UpdatedBy);
        }
    }
}
