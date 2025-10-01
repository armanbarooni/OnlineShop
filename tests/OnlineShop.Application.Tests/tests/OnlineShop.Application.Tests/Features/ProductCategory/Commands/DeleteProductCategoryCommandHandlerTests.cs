using Xunit;
using Moq;
using FluentAssertions;
using OnlineShop.Application.Features.ProductCategory.Command.Delete;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Commands
{
    public class DeleteProductCategoryCommandHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;

        public DeleteProductCategoryCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
        }

        [Fact]
        public async Task Handle_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Domain.Entities.ProductCategory.Create(
                "Electronics", 
                "Electronic products", 
                1000, 
                100
            );

            _mockRepository.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCategory);

            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new DeleteProductCategoryCommandHandler(_mockRepository.Object);
            var command = new DeleteProductCategoryCommand(categoryId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.DeleteAsync(existingCategory, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidId_ReturnsFailure()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.ProductCategory?)null);

            var handler = new DeleteProductCategoryCommandHandler(_mockRepository.Object);
            var command = new DeleteProductCategoryCommand(categoryId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("not found");
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
