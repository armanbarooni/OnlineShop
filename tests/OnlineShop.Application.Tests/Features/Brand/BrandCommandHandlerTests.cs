using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using OnlineShop.Application.Features.Brand.Commands.Create;
using OnlineShop.Application.Features.Brand.Commands.Update;
using OnlineShop.Application.Features.Brand.Commands.Delete;
using OnlineShop.Application.DTOs.Brand;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Features.Brand
{
    public class BrandCommandHandlerTests
    {
        private readonly Mock<IBrandRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;

        public BrandCommandHandlerTests()
        {
            _mockRepository = new Mock<IBrandRepository>();
            _mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task CreateBrand_ValidRequest_ShouldSucceed()
        {
            // Arrange
            var dto = new CreateBrandDto { Name = "Nike", Description = "Just Do It" };
            var brand = OnlineShop.Domain.Entities.Brand.Create("Nike", null, "Just Do It");

            _mockMapper.Setup(m => m.Map<OnlineShop.Domain.Entities.Brand>(dto))
                .Returns(brand);

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.Brand>(), It.IsAny<CancellationToken>()))
                .Returns((OnlineShop.Domain.Entities.Brand b, CancellationToken ct) => Task.FromResult(b));

            var handler = new CreateBrandCommandHandler(_mockRepository.Object, _mockMapper.Object);
            var command = new CreateBrandCommand { Brand = dto };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.Brand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBrand_ValidRequest_ShouldSucceed()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var dto = new UpdateBrandDto { Id = brandId, Name = "Nike Updated" };
            var existingBrand = OnlineShop.Domain.Entities.Brand.Create("Nike", null, "Old Description");

            _mockRepository.Setup(r => r.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingBrand);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Brand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new UpdateBrandCommandHandler(_mockRepository.Object, _mockMapper.Object);
            var command = new UpdateBrandCommand { Brand = dto };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Brand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBrand_NotFound_ShouldReturnFailure()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var dto = new UpdateBrandDto { Id = brandId, Name = "Nike" };

            _mockRepository.Setup(r => r.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Brand?)null);

            var handler = new UpdateBrandCommandHandler(_mockRepository.Object, _mockMapper.Object);
            var command = new UpdateBrandCommand { Brand = dto };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteBrand_ValidId_ShouldSucceed()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var brand = OnlineShop.Domain.Entities.Brand.Create("Nike", null, null);

            _mockRepository.Setup(r => r.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(brand);

            _mockRepository.Setup(r => r.DeleteAsync(brandId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteBrandCommandHandler(_mockRepository.Object);
            var command = new DeleteBrandCommand(brandId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.DeleteAsync(brandId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteBrand_NotFound_ShouldReturnFailure()
        {
            // Arrange
            var brandId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Brand?)null);

            var handler = new DeleteBrandCommandHandler(_mockRepository.Object);
            var command = new DeleteBrandCommand(brandId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }
    }
}

