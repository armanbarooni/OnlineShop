using AutoMapper;
using FluentAssertions;
using Moq;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Command.Update;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Tests.Application.Features.Product.Command.Update
{
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateProductCommandHandler _handler;

        public UpdateProductCommandHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateProductCommandHandler(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateProductAndReturnSuccess()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updateDto = new UpdateProductDto
            {
                Id = productId,
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 150m,
                StockQuantity = 75
            };

            var command = new UpdateProductCommand { Product = updateDto };

            var existingProduct = Domain.Entities.Product.Create(
                "Original Product",
                "Original Description",
                100m,
                50
            );

            var productDto = new ProductDto
            {
                Id = productId,
                Name = updateDto.Name,
                Price = updateDto.Price
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Domain.Entities.Product>()))
                .Returns(productDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Name.Should().Be(updateDto.Name);

            _repositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ShouldReturnFailure()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updateDto = new UpdateProductDto
            {
                Id = productId,
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 150m,
                StockQuantity = 75
            };

            var command = new UpdateProductCommand { Product= updateDto };

            _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Product)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Product not found"); // تغییر از Error به Message

            _repositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updateDto = new UpdateProductDto
            {
                Id = productId,
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 150m,
                StockQuantity = 75
            };

            var command = new UpdateProductCommand{ Product = updateDto };

            var existingProduct = Domain.Entities.Product.Create(
                "Original Product",
                "Original Description",
                100m,
                50
            );

            _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        }
    }
}
