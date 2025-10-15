using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using OnlineShop.Application.Features.Unit.Queries.GetById;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Domain.Entities;
using OnlineShop.Application.Exceptions;

namespace OnlineShop.Application.Tests.Features.Unit.Queries
{
    public class GetUnitByIdQueryHandlerTests
    {
        private readonly Mock<IUnitRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetUnitByIdQueryHandler _handler;

        public GetUnitByIdQueryHandlerTests()
        {
            _mockRepository = new Mock<IUnitRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetUnitByIdQueryHandler(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsUnitDetails()
        {
            // Arrange
            var unitId = Guid.NewGuid();
            var unit = OnlineShop.Domain.Entities.Unit.Create(1, "Kilogram", 123, 1, "Weight unit");

            var unitDetailsDto = new UnitDetailsDto
            {
                Id = unitId,
                Name = "Kilogram",
                Comment = "Weight unit"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(unit);

            _mockMapper.Setup(m => m.Map<UnitDetailsDto>(unit))
                .Returns(unitDetailsDto);

            var query = new GetUnitByIdQuery { Id = unitId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(unitId);
            result.Data.Name.Should().Be("Kilogram");

            _mockRepository.Verify(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()), Times.Once);
            _mockMapper.Verify(m => m.Map<UnitDetailsDto>(unit), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidId_ThrowsNotFoundException()
        {
            // Arrange
            var unitId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Unit)null);

            var query = new GetUnitByIdQuery { Id = unitId };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _handler.Handle(query, CancellationToken.None));

            _mockRepository.Verify(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()), Times.Once);
            _mockMapper.Verify(m => m.Map<UnitDetailsDto>(It.IsAny<OnlineShop.Domain.Entities.Unit>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyGuid_ThrowsNotFoundException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            _mockRepository.Setup(r => r.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Unit)null);

            var query = new GetUnitByIdQuery { Id = emptyGuid };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _handler.Handle(query, CancellationToken.None));
        }
    }
}
