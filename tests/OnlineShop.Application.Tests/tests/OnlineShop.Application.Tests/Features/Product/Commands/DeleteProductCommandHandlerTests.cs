using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Features.Product.Command.Delete;
using OnlineShop.Infrastructure.Persistence.Repositories;
using Xunit;

namespace OnlineShop.Application.Tests.Features.Product.Commands
{
    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _mockRepository;

        public DeleteProductCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductRepository>();
        }

        [Fact]
        public async Task Handle_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            
            var existingProduct = Domain.Entities.Product.Create("Test Product", "Description", 100m, unitId, categoryId, 1000, 100);

            _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteProductCommandHandler(_mockRepository.Object);
            var command = new DeleteProductCommand(productId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
