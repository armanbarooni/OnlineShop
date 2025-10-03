using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using OnlineShop.Application.Features.Unit.Command.Create;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Features.Unit.Commands
{
    public class CreateUnitCommandHandlerTests
    {
        private readonly Mock<IUnitRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CreateUnitCommandHandler _handler;

        public CreateUnitCommandHandlerTests()
        {
            _mockRepository = new Mock<IUnitRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new CreateUnitCommandHandler(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_ValidUnit_CreatesSuccessfully()
        {
            // Arrange
            var createDto = new CreateUnitDto
            {
                Name = "Kilogram",
                Comment = "Weight unit"
            };

            var command = new CreateUnitCommand
            {
                UnitDto = createDto
            };

            var unitId = Guid.NewGuid();

            // تغییر: از factory method استفاده می‌کنیم
            var unitEntity = Domain.Entities.Unit.Create(
                unitCode: 1,
                name: "Kilogram",
                mahakClientId: 123,
                mahakId: 1,
                comment: "Weight unit"
            );

            // Id را manually ست می‌کنیم (از reflection استفاده می‌کنیم)
            var idProperty = typeof(Domain.Entities.Unit).GetProperty("Id");
            idProperty?.SetValue(unitEntity, unitId);

            _mockRepository.Setup(r => r.ExistsByNameAsync(createDto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMapper.Setup(m => m.Map<Domain.Entities.Unit>(createDto))
                .Returns(unitEntity);

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Unit>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBe(Guid.Empty);

            _mockRepository.Verify(r => r.ExistsByNameAsync(createDto.Name, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Unit>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateName_ReturnsFailure()
        {
            // Arrange
            var createDto = new CreateUnitDto
            {
                Name = "Kilogram",
                Comment = "Weight unit"
            };

            var command = new CreateUnitCommand
            {
                UnitDto = createDto
            };

            _mockRepository.Setup(r => r.ExistsByNameAsync(createDto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Unit>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NullDto_ReturnsFailure()
        {
            // Arrange
            var command = new CreateUnitCommand
            {
                UnitDto = null
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EmptyName_ReturnsFailure()
        {
            // Arrange
            var createDto = new CreateUnitDto
            {
                Name = "",
                Comment = "Weight unit"
            };

            var command = new CreateUnitCommand
            {
                UnitDto = createDto
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }
    }
}
