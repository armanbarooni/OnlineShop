using AutoMapper;
using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Mapping;
using OnlineShop.Application.Features.Product.Queries.GetAll;
using Xunit;

namespace OnlineShop.Application.Tests.Features.Product.Queries
{
    public class GetAllProductsQueryHandlerTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly IMapper _mapper;

        public GetAllProductsQueryHandlerTests()
        {
            _mockRepository = new Mock<IProductRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_ReturnsAllProducts()
        {
            // Arrange
            var unitId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            
            var product1 = Domain.Entities.Product.Create("Product 1", "Desc 1", 10m, unitId, categoryId, 1000, 100);
            var product2 = Domain.Entities.Product.Create("Product 2", "Desc 2", 20m, unitId, categoryId, 1000, 101);
            
            var products = new List<Domain.Entities.Product> { product1, product2 };

            _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            var handler = new GetAllProductsQueryHandler(_mockRepository.Object, _mapper);
            var query = new GetAllProductsQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }
    }
}
