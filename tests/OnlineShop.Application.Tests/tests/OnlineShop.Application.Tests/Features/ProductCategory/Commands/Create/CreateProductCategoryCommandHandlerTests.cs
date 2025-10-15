using Moq;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Features.ProductCategory.Command.Create;
using OnlineShop.Domain.Entities;
using Xunit;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Commands.Create
{
    public class CreateProductCategoryCommandHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;
        private readonly CreateProductCategoryCommandHandler _handler;

        public CreateProductCategoryCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
            _handler = new CreateProductCategoryCommandHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessResult()
        {
            // Arrange
            var command = new CreateProductCategoryCommand
            {
                Dto = new CreateProductCategoryDto
                {
                    Name = "Electronics",
                    Description = "Electronic products"
                }
            };

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Electronics", result.Data.Name);
            Assert.Equal("Electronic products", result.Data.Description);
            // No Mahak fields assertions

            _mockRepository.Verify(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsRepositoryMethods()
        {
            // Arrange
            var command = new CreateProductCategoryCommand
            {
                Dto = new CreateProductCategoryDto
                {
                    Name = "Books",
                    Description = "Book category"
                }
            };

            OnlineShop.Domain.Entities.ProductCategory? capturedEntity = null;
            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()))
                .Callback<OnlineShop.Domain.Entities.ProductCategory, CancellationToken>((entity, ct) => capturedEntity = entity)
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedEntity);
            Assert.Equal("Books", capturedEntity.Name);
            Assert.Equal("Book category", capturedEntity.Description);
            // No Mahak fields assertions
        }

        [Fact]
        public async Task Handle_EmptyDescription_ReturnsSuccessResult()
        {
            // Arrange
            var command = new CreateProductCategoryCommand
            {
                Dto = new CreateProductCategoryDto
                {
                    Name = "Clothing",
                    Description = ""
                }
            };

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Clothing", result.Data.Name);
            Assert.Equal("", result.Data.Description);
        }
    }
}
