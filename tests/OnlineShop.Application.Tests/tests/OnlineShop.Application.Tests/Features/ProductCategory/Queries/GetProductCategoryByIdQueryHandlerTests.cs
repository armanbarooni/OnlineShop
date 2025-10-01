using AutoMapper;
using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Mapping;
using OnlineShop.Application.Features.ProductCategory.Queries.GetById;
using Xunit;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Queries
{
    public class GetProductCategoryByIdQueryHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;
        private readonly IMapper _mapper;

        public GetProductCategoryByIdQueryHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductCategoryProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = Domain.Entities.ProductCategory.Create("Electronics", "Electronic products", 1000, 100);
            
            _mockRepository.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            var handler = new GetProductCategoryByIdQueryHandler(_mockRepository.Object, _mapper);
            var query = new GetProductCategoryByIdQuery(categoryId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Name.Should().Be("Electronics");
        }
    }
}
