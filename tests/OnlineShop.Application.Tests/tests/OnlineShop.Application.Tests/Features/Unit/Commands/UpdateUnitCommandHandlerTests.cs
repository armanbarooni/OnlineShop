using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using OnlineShop.Application.Features.Unit.Command.Update;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Features.Unit.Commands
{
    public class UpdateUnitCommandHandlerTests
    {
        private readonly Mock<IUnitRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UpdateUnitCommandHandler _handler;

        public UpdateUnitCommandHandlerTests()
        {
            _mockRepository = new Mock<IUnitRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new UpdateUnitCommandHandler(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_ValidUpdate_UpdatesSuccessfully()
        {
            // Arrange
            var unitId = Guid.NewGuid();
            var updateDto = new UpdateUnitDto
            {
                Id = unitId,
                Name = "Gram",
                Comment = "Updated weight unit"
            };

            var command = new UpdateUnitCommand
            {
                UnitDto = updateDto
            };

            var existingUnit = Domain.Entities.Unit.Create(1, "Kilogram", 123, 1, "Weight unit");

            _mockRepository.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUnit);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.Unit>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(r => r.ExistsByNameAsync(updateDto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();

            _mockRepository.Verify(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Unit>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsFailure()
        {
            // Arrange
            var unitId = Guid.NewGuid();
            var updateDto = new UpdateUnitDto
            {
                Id = unitId,
                Name = "Gram",
                Comment = "Updated weight unit"
            };

            var command = new UpdateUnitCommand
            {
                UnitDto = updateDto
            };

            _mockRepository.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Unit)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();

            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Unit>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DuplicateName_ReturnsFailure()
        {
            // Arrange
            var unitId = Guid.NewGuid();
            var updateDto = new UpdateUnitDto
            {
                Id = unitId,
                Name = "Gram",
                Comment = "Updated weight unit"
            };

            var command = new UpdateUnitCommand
            {
                UnitDto = updateDto
            };

            var existingUnit = Domain.Entities.Unit.Create(1, "Kilogram", 123, 1, "Weight unit");

            _mockRepository.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUnit);

            _mockRepository.Setup(r => r.ExistsByNameAsync(updateDto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();

            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Unit>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NullDto_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateUnitCommand
            {
                UnitDto = null
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_EmptyName_ReturnsFailure()
        {
            // Arrange
            var unitId = Guid.NewGuid();
            var updateDto = new UpdateUnitDto
            {
                Id = unitId,
                Name = "",
                Comment = "Updated weight unit"
            };

            var command = new UpdateUnitCommand
            {
                UnitDto = updateDto
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }
    }
}
