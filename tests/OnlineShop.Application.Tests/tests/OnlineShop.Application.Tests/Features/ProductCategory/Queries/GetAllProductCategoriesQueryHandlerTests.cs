using AutoMapper;
using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Mapping;
using OnlineShop.Application.Features.ProductCategory.Queries.GetAll;
using Xunit;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Queries
{
    public class GetAllProductCategoriesQueryHandlerTests
    {
        private readonly Mock<IProductCategoryRepository> _mockRepository;
        private readonly IMapper _mapper;

        public GetAllProductCategoriesQueryHandlerTests()
        {
            _mockRepository = new Mock<IProductCategoryRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductCategoryProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_ReturnsAllCategories()
        {
            // Arrange
            var category1 = Domain.Entities.ProductCategory.Create("Electronics", "Electronic products", 1000, 100);
            var category2 = Domain.Entities.ProductCategory.Create("Clothing", "Clothing items", 1000, 101);
            
            var categories = new List<Domain.Entities.ProductCategory> { category1, category2 };

            _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            var handler = new GetAllProductCategoriesQueryHandler(_mockRepository.Object, _mapper);
            var query = new GetAllProductCategoriesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }
    }
}
