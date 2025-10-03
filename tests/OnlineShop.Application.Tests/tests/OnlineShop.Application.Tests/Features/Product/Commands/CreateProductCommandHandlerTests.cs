using AutoMapper;
using FluentAssertions;
using Moq;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Command.Create;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Tests.Application.Features.Product.Command.Create
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateProductCommandHandler _handler;

        public CreateProductCommandHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateProductCommandHandler(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateProductAndReturnSuccess()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100m,
                StockQuantity = 50
            };

            var command = new CreateProductCommand { Product= createDto };

            var productDto = new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Price = createDto.Price
            };

            _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Domain.Entities.Product>()))
                .Returns(productDto);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Name.Should().Be(createDto.Name);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductDto>(It.IsAny<Domain.Entities.Product>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100m,
                StockQuantity = 50
            };

            var command = new CreateProductCommand { Product = createDto };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCallRepositoryWithCorrectProduct()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100m,
                StockQuantity = 50
            };

            var command = new CreateProductCommand { Product = createDto };
            Domain.Entities.Product capturedProduct = null;

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Callback<Domain.Entities.Product, CancellationToken>((p, ct) => capturedProduct = p)
                .Returns(Task.CompletedTask);

            _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Domain.Entities.Product>()))
                .Returns(new ProductDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedProduct.Should().NotBeNull();
            capturedProduct.Name.Should().Be(createDto.Name);
            capturedProduct.Description.Should().Be(createDto.Description);
            capturedProduct.Price.Should().Be(createDto.Price);
            capturedProduct.StockQuantity.Should().Be(createDto.StockQuantity);
        }
    }
}
