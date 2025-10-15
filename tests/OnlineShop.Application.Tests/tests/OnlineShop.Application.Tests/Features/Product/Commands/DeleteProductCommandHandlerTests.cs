using Xunit;
using Moq;
using OnlineShop.Application.Features.Product.Command.Delete;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

namespace OnlineShop.UnitTests.Application.Features.Product.Commands.Delete
{
    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly DeleteProductCommandHandler _handler;

        public DeleteProductCommandHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _handler = new DeleteProductCommandHandler(_mockProductRepository.Object);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ShouldReturnFailure()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Product)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Product not found"); // ÇÓÊÝÇÏå ÇÒ Error Èå ÌÇí Message

            _mockProductRepository.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductRepository.Verify(x => x.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldDeleteProductAndReturnSuccess()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);

            var existingProduct = OnlineShop.Domain.Entities.Product.Create(
                "Test Product",
                "Description",
                100m,
                10
            );

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Contain("deleted successfully");

            _mockProductRepository.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductRepository.Verify(x => x.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }

    }
}
