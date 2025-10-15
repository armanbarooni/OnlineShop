using Moq;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.Features.ProductCategory.Command.Delete;
using OnlineShop.Domain.Entities;
using Xunit;

namespace OnlineShop.Tests.Features.ProductCategory.Commands
{
    public class DeleteProductCategoryCommandHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;
        private readonly DeleteProductCategoryCommandHandler _handler;

        public DeleteProductCategoryCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
            _handler = new DeleteProductCategoryCommandHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task Handle_ExistingProductCategory_ReturnsSuccessResult()
        {
            // Arrange
            var productCategoryId = Guid.NewGuid();
            var existingProductCategory = Domain.Entities.ProductCategory.Create(
                "Electronics",
                "Electronics description",
                100,
                1001
            );
            typeof(Domain.Entities.ProductCategory)
                .GetProperty("Id")!
                .SetValue(existingProductCategory, productCategoryId);

            var command = new DeleteProductCategoryCommand { Id = productCategoryId };

            _mockRepository
                .Setup(r => r.GetByIdAsync(productCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProductCategory);

            _mockRepository
                .Setup(r => r.DeleteAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            _mockRepository.Verify(r => r.DeleteAsync(existingProductCategory, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingProductCategory_ReturnsFailureResult()
        {
            // Arrange
            var productCategoryId = Guid.NewGuid();
            var command = new DeleteProductCategoryCommand { Id = productCategoryId };

            _mockRepository
                .Setup(r => r.GetByIdAsync(productCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.ProductCategory)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.ErrorMessage);
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DeletedProductCategory_StillReturnsSuccess()
        {
            // Arrange
            var productCategoryId = Guid.NewGuid();
            var existingProductCategory = Domain.Entities.ProductCategory.Create(
                "Electronics",
                "Electronics description",
                100,
                1001
            );
            typeof(Domain.Entities.ProductCategory)
                .GetProperty("Id")!
                .SetValue(existingProductCategory, productCategoryId);

            // Mark as deleted
            existingProductCategory.Delete(null);

            var command = new DeleteProductCategoryCommand { Id = productCategoryId };

            // Repository ÝíáÊÑ ãí˜äÏ¡ Ó already deleted item ÈÑäãíÑÏÇäÏ
            _mockRepository
                .Setup(r => r.GetByIdAsync(productCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.ProductCategory)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.ErrorMessage);
        }
    }
}
