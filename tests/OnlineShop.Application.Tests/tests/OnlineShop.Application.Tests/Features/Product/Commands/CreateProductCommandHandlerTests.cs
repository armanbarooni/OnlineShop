using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Command.Create;
using Xunit;

namespace OnlineShop.Application.Tests.Features.Product.Commands
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _mockRepository;

        public CreateProductCommandHandlerTests()
        {
            _mockRepository = new Mock<IProductRepository>();
        }

        [Fact]
        public async Task Handle_ValidProduct_CreatesSuccessfully()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100.50m,
                UnitId = Guid.NewGuid(),
                ProductCategoryId = Guid.NewGuid(),
                MahakId = 100,
                MahakClientId = 1000
            };

            var newProductId = Guid.NewGuid();
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newProductId);

            var handler = new CreateProductCommandHandler(_mockRepository.Object);
            var command = new CreateProductCommand(createDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(newProductId);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
