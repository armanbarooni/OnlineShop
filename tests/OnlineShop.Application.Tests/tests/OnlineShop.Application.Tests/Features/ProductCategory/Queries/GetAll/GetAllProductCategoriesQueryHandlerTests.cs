using Moq;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.Features.ProductCategory.Queries.GetAll;
using OnlineShop.Domain.Entities;
using Xunit;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Queries.GetAll
{
    public class GetAllProductCategoriesQueryHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;
        private readonly GetAllProductCategoriesQueryHandler _handler;

        public GetAllProductCategoriesQueryHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
            _handler = new GetAllProductCategoriesQueryHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task Handle_ReturnsAllProductCategories()
        {
            // Arrange
            var categories = new List<OnlineShop.Domain.Entities.ProductCategory>
            {
                OnlineShop.Domain.Entities.ProductCategory.Create("Electronics", "Electronic products", 1001, 2001),
                OnlineShop.Domain.Entities.ProductCategory.Create("Books", "Book products", 1002, 2002),
                OnlineShop.Domain.Entities.ProductCategory.Create("Clothing", "Clothing products", 1003, 2003)
            };

            var query = new GetAllProductCategoriesQuery();

            _mockRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count());

            var dataList = result.Data.ToList();
            Assert.Equal("Electronics", dataList[0].Name);
            Assert.Equal("Books", dataList[1].Name);
            Assert.Equal("Clothing", dataList[2].Name);

            _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmptyResult()
        {
            // Arrange
            var query = new GetAllProductCategoriesQuery();

            _mockRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OnlineShop.Domain.Entities.ProductCategory>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SingleCategory_ReturnsOneResult()
        {
            // Arrange
            var categories = new List<OnlineShop.Domain.Entities.ProductCategory>
            {
                OnlineShop.Domain.Entities.ProductCategory.Create("Single Category", "Only one", 1004, 2004)
            };

            var query = new GetAllProductCategoriesQuery();

            _mockRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("Single Category", result.Data.First().Name);
        }
    }
}
