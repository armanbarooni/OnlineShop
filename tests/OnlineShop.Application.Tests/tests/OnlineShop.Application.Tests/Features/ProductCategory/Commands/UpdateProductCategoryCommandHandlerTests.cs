using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Features.ProductCategory.Command.Update;
using Xunit;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Commands
{
    public class UpdateProductCategoryCommandHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;

        public UpdateProductCategoryCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
        }

        [Fact]
        public async Task Handle_ValidCategory_UpdatesSuccessfully()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Domain.Entities.ProductCategory.Create("Old Electronics", "Old Description", 1000, 100);

            var updateDto = new UpdateProductCategoryDto
            {
                Name = "Updated Electronics",
                Description = "Updated Description",
                MahakId = 200,
                MahakClientId = 2000
            };

            _mockRepository.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCategory);

            _mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new UpdateProductCategoryCommandHandler(_mockRepository.Object);
            var command = new UpdateProductCategoryCommand(categoryId, updateDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
