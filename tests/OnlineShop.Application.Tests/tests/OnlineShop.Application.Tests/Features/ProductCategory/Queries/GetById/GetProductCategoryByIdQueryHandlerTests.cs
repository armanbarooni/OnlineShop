using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Features.ProductCategory.Queries.GetById;
using OnlineShop.Domain.Entities;
using Xunit;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Queries
{
    public class GetProductCategoryByIdQueryHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;
        private readonly GetProductCategoryByIdQueryHandler _handler;

        public GetProductCategoryByIdQueryHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
            _handler = new GetProductCategoryByIdQueryHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task Handle_ExistingProductCategory_ReturnsProductCategoryDetailsDto()
        {
            // Arrange
            var productCategoryId = Guid.NewGuid();
            var productCategory = Domain.Entities.ProductCategory.Create(
                "Electronics",
                "Electronics description",
                100,
                1001
            );
            typeof(Domain.Entities.ProductCategory)
                .GetProperty("Id")!
                .SetValue(productCategory, productCategoryId);

            var query = new GetProductCategoryByIdQuery { Id = productCategoryId };

            _mockRepository
                .Setup(r => r.GetByIdAsync(productCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productCategory);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(productCategoryId, result.Data.Id);
            Assert.Equal("Electronics", result.Data.Name);
            Assert.Equal("Electronics description", result.Data.Description);
            // Mahak fields are not part of public contract assertions
        }

        [Fact]
        public async Task Handle_NonExistingProductCategory_ReturnsFailureResult()
        {
            // Arrange
            var productCategoryId = Guid.NewGuid();
            var query = new GetProductCategoryByIdQuery { Id = productCategoryId };

            _mockRepository
                .Setup(r => r.GetByIdAsync(productCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.ProductCategory)null!);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.ErrorMessage);
            Assert.Null(result.Data);
        }
    }
}
