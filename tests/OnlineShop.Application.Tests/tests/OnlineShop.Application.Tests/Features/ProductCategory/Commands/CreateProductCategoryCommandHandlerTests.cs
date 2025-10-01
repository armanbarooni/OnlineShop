using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Features.ProductCategory.Command.Create;
using Xunit;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Commands
{
    public class CreateProductCategoryCommandHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;

        public CreateProductCategoryCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
        }

        [Fact]
        public async Task Handle_ValidCategory_CreatesSuccessfully()
        {
            // Arrange
            var createDto = new CreateProductCategoryDto
            {
                Name = "Electronics",
                Description = "Electronic products",
                MahakId = 100,
                MahakClientId = 1000
            };

            var newCategoryId = Guid.NewGuid();
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newCategoryId);

            var handler = new CreateProductCategoryCommandHandler(_mockRepository.Object);
            var command = new CreateProductCategoryCommand(createDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(newCategoryId);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.ProductCategory>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
