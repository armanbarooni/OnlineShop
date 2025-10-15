// GetProductByIdQueryHandlerTests.cs
using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Application.Features.Product.Queries.GetById;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Tests.TestHelpers;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Features.Product.Queries.GetById
{
    public class GetProductByIdQueryHandlerTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly GetProductByIdQueryHandler _handler;

        public GetProductByIdQueryHandlerTests()
        {
            _mockRepository = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockMediator = new Mock<IMediator>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _handler = new GetProductByIdQueryHandler(
                _mockRepository.Object, 
                _mockMapper.Object,
                _mockMediator.Object,
                _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task Handle_ProductExists_ShouldReturnSuccess()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = ProductMother.CreateWithId(productId, "Test Product", "Test Description", 100m, 10);

            var productDetailsDto = new ProductDetailsDto
            {
                Id = productId,
                Name = "Test Product",
                Description = "Test Description",
                Price = 100m,
                StockQuantity = 10
            };

            _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _mockMapper.Setup(m => m.Map<ProductDetailsDto>(product))
                .Returns(productDetailsDto);

            var query = new GetProductByIdQuery { Id = productId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(productId);
            result.Data.Name.Should().Be("Test Product");
            result.Data.Price.Should().Be(100m);

            _mockRepository.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _mockMapper.Verify(m => m.Map<ProductDetailsDto>(product), Times.Once);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ShouldReturnFailure()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Product?)null);

            var query = new GetProductByIdQuery { Id = productId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty(); // ������� �� ErrorMessage
            result.ErrorMessage.Should().Contain("not found");

            _mockRepository.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _mockMapper.Verify(m => m.Map<ProductDetailsDto>(It.IsAny<OnlineShop.Domain.Entities.Product>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyGuid_ShouldReturnFailure()
        {
            // Arrange
            var query = new GetProductByIdQuery { Id = Guid.Empty };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty(); // ������� �� ErrorMessage
        }
    }
}
