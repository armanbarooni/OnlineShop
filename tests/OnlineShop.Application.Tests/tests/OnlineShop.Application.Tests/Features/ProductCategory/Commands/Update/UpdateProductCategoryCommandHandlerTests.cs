using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Features.ProductCategory.Command.Update;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Domain.Entities;
using Xunit;

namespace OnlineShop.Tests.Features.ProductCategory.Commands
{
    public class UpdateProductCategoryCommandHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;
        private readonly UpdateProductCategoryCommandHandler _handler;

        public UpdateProductCategoryCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
            _handler = new UpdateProductCategoryCommandHandler(_mockRepository.Object);
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

            var command = new UpdateProductCategoryCommand
            {
               
                Dto = new UpdateProductCategoryDto
                {
                    Id = productCategoryId,
                    Name = "Updated Electronics",
                    Description = "Updated description"   
                }
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(productCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProductCategory);

            _mockRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated Electronics", result.Data.Name);
            Assert.Equal("Updated description", result.Data.Description);
            // این فیلدها باید بدون تغییر باقی بمانند
            Assert.Equal(100, result.Data.MahakClientId);
            Assert.Equal(1001, result.Data.MahakId);
        }

        [Fact]
        public async Task Handle_NonExistingProductCategory_ReturnsFailureResult()
        {
            // Arrange
            var productCategoryId = Guid.NewGuid();
            var command = new UpdateProductCategoryCommand
            {
              
                Dto = new UpdateProductCategoryDto
                {
                    Id = productCategoryId,
                    Name = "Non-existing",
                    Description = "Non-existing description"
                }
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(productCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.ProductCategory)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.ErrorMessage);
            Assert.Null(result.Data);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdateCallsCapturesCorrectEntity()
        {
            // Arrange
            var productCategoryId = Guid.NewGuid();
            var originalMahakClientId = 300L;
            var originalMahakId = 1004;

            var existingProductCategory = Domain.Entities.ProductCategory.Create(
                "Books",
                "Books description",
                originalMahakClientId,
                originalMahakId
            );
            typeof(Domain.Entities.ProductCategory)
                .GetProperty("Id")!
                .SetValue(existingProductCategory, productCategoryId);

            var command = new UpdateProductCategoryCommand
            {
            
                Dto = new UpdateProductCategoryDto
                {
                    Id = productCategoryId,
                    Name = "Updated Books",
                    Description = "Updated books description"
                }
            };

            Domain.Entities.ProductCategory? capturedEntity = null;
            _mockRepository
                .Setup(r => r.GetByIdAsync(productCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProductCategory);

            _mockRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()))
                .Callback<Domain.Entities.ProductCategory, CancellationToken>((entity, ct) => capturedEntity = entity)
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEntity);
            Assert.Equal("Updated Books", capturedEntity.Name);
            Assert.Equal("Updated books description", capturedEntity.Description);
            // فیلدهای Mahak باید بدون تغییر باشند
            Assert.Equal(originalMahakClientId, capturedEntity.MahakClientId);
            Assert.Equal(originalMahakId, capturedEntity.MahakId);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
