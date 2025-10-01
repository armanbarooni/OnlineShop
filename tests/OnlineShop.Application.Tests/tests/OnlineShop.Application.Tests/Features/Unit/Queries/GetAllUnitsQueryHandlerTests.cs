using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using OnlineShop.Application.Features.Unit.Queries.GetAllUnits;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Domain.Entites;

namespace OnlineShop.Application.Tests.Features.Unit.Query
{
    public class GetAllUnitsQueryHandlerTests
    {
        private readonly Mock<IUnitRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetAllUnitsQueryHandler _handler;

        public GetAllUnitsQueryHandlerTests()
        {
            _mockRepository = new Mock<IUnitRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetAllUnitsQueryHandler(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_ReturnsAllUnits_Successfully()
        {
            // Arrange
            var units = new List<Domain.Entites.Unit>
            {
                Domain.Entites.Unit.Create(1, "Kilogram", 123, 1, "Weight unit"),
                Domain.Entites.Unit.Create(2, "Meter", 124, 2, "Length unit")
            };

            var unitDtos = new List<UnitDto>
            {
                new UnitDto { Id = Guid.NewGuid(), Name = "Kilogram" },
                new UnitDto { Id = Guid.NewGuid(), Name = "Meter" }
            };

            _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(units);

            _mockMapper.Setup(m => m.Map<List<UnitDto>>(units))
                .Returns(unitDtos);

            var query = new GetAllUnitsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(unitDtos);

            _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockMapper.Verify(m => m.Map<List<UnitDto>>(units), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmptyResult()
        {
            // Arrange
            var emptyUnits = new List<Domain.Entites.Unit>();
            var emptyDtos = new List<UnitDto>();

            _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyUnits);

            _mockMapper.Setup(m => m.Map<List<UnitDto>>(emptyUnits))
                .Returns(emptyDtos);

            var query = new GetAllUnitsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();

            _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
