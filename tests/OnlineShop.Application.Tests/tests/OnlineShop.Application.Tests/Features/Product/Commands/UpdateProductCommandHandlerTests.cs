using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Command.Update;
using Xunit;

namespace OnlineShop.Application.Tests.Features.Product.Commands
{
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _mockRepository;

        public UpdateProductCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductRepository>();
        }

        [Fact]
        public async Task Handle_ValidProduct_UpdatesSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            
            var existingProduct = Domain.Entities.Product.Create("Old Name", "Old Desc", 50m, unitId, categoryId, 1000, 100);

            var updateDto = new UpdateProductDto
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 150.75m,
                UnitId = unitId,
                ProductCategoryId = categoryId,
                MahakId = 100,
                MahakClientId = 1000
            };

            _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new UpdateProductCommandHandler(_mockRepository.Object);
            var command = new UpdateProductCommand(updateDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
