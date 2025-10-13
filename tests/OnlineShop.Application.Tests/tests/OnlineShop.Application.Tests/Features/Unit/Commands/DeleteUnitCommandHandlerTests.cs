using Xunit;
using Moq;
using FluentAssertions;
using OnlineShop.Application.Features.Unit.Command.Delete;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Features.Unit.Commands
{
    public class DeleteUnitCommandHandlerTests
    {
        private readonly Mock<IUnitRepository> _mockRepository;
        private readonly DeleteUnitCommandHandler _handler;

        public DeleteUnitCommandHandlerTests()
        {
            _mockRepository = new Mock<IUnitRepository>();
            _handler = new DeleteUnitCommandHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var unitId = Guid.NewGuid();
            var unit = OnlineShop.Domain.Entities.Unit.Create(1, "Kilogram", 123, 1, "Weight unit");

            _mockRepository.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(unit);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Unit>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new DeleteUnitCommand { Id = unitId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();

            _mockRepository.Verify(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Unit>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidId_ReturnsFailure()
        {
            // Arrange
            var unitId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Unit)null);

            var command = new DeleteUnitCommand { Id = unitId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("واحد مورد نظر پیدا نشد.");

            _mockRepository.Verify(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Unit>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyGuid_ReturnsFailure()
        {
            // Arrange
            var command = new DeleteUnitCommand { Id = Guid.Empty };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("شناسه واحد معتبر نیست.");

            _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Unit>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
