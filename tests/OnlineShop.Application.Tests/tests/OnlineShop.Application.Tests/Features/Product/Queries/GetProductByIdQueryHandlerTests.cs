using AutoMapper;
using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Mapping;
using OnlineShop.Application.Features.Product.Queries.GetById;
using Xunit;

namespace OnlineShop.Application.Tests.Features.Product.Queries
{
    public class GetProductByIdQueryHandlerTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandlerTests()
        {
            _mockRepository = new Mock<IProductRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            
            var product = Domain.Entities.Product.Create("Test Product", "Description", 99.99m, unitId, categoryId, 1000, 100);
            
            _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var handler = new GetProductByIdQueryHandler(_mockRepository.Object, _mapper);
            var query = new GetProductByIdQuery(productId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Name.Should().Be("Test Product");
        }
    }
}
