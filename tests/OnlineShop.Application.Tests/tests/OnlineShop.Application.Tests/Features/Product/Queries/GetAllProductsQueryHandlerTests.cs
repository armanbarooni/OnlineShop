using AutoMapper;
using FluentAssertions;
using Moq;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Queries.GetAll;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Tests.Features.Product.Queries.GetAll
{
    public class GetAllProductsQueryHandlerTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAllProductsQueryHandler _handler;

        public GetAllProductsQueryHandlerTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetAllProductsQueryHandler(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task HandleValidRequestShouldReturnAllProducts()
        {
            // Arrange
            var query = new GetAllProductsQuery();

            var products = new List<Domain.Entities.Product>
            {
                Domain.Entities.Product.Create("Product 1", "Desc 1", 100m, 10),
                Domain.Entities.Product.Create("Product 2", "Desc 2", 200m, 20)
            };

            var productDtos = new List<ProductDto>
            {
                new ProductDto { Id = Guid.NewGuid(), Name = "Product 1", Price = 100m },
                new ProductDto { Id = Guid.NewGuid(), Name = "Product 2", Price = 200m }
            };

            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            _mapperMock.Setup(m => m.Map<IEnumerable<ProductDto>>(products))
                .Returns(productDtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.First().Name.Should().Be("Product 1");

            _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ProductDto>>(products), Times.Once);
        }

        [Fact]
        public async Task Handle_NoProducts_ShouldReturnEmptyList()
        {
            // Arrange
            var query = new GetAllProductsQuery();
            var emptyList = new List<Domain.Entities.Product>();

            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);

            _mapperMock.Setup(m => m.Map<IEnumerable<ProductDto>>(emptyList))
                .Returns(new List<ProductDto>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();

            _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var query = new GetAllProductsQuery();

            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        }
    }
}
