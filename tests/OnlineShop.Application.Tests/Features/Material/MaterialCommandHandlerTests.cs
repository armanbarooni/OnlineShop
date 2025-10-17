using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using OnlineShop.Application.Features.Material.Commands.Create;
using OnlineShop.Application.Features.Material.Commands.Update;
using OnlineShop.Application.Features.Material.Commands.Delete;
using OnlineShop.Application.DTOs.Material;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Tests.Features.Material
{
    public class MaterialCommandHandlerTests
    {
        private readonly Mock<IMaterialRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;

        public MaterialCommandHandlerTests()
        {
            _mockRepository = new Mock<IMaterialRepository>();
            _mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task CreateMaterial_ValidRequest_ShouldSucceed()
        {
            var dto = new CreateMaterialDto { Name = "Cotton", Description = "100% Cotton" };
            var material = OnlineShop.Domain.Entities.Material.Create("Cotton", "100% Cotton");

            _mockMapper.Setup(m => m.Map<OnlineShop.Domain.Entities.Material>(dto)).Returns(material);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.Material>(), It.IsAny<CancellationToken>()))
                .Returns((OnlineShop.Domain.Entities.Material m, CancellationToken ct) => Task.FromResult(m));

            var handler = new CreateMaterialCommandHandler(_mockRepository.Object, _mockMapper.Object);
            var command = new CreateMaterialCommand { Material = dto };

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.Material>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMaterial_ValidRequest_ShouldSucceed()
        {
            var materialId = Guid.NewGuid();
            var dto = new UpdateMaterialDto { Id = materialId, Name = "Cotton Updated" };
            var existingMaterial = OnlineShop.Domain.Entities.Material.Create("Cotton", "Old");

            _mockRepository.Setup(r => r.GetByIdAsync(materialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingMaterial);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Material>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new UpdateMaterialCommandHandler(_mockRepository.Object, _mockMapper.Object);
            var command = new UpdateMaterialCommand { Material = dto };

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateMaterial_NotFound_ShouldReturnFailure()
        {
            var materialId = Guid.NewGuid();
            var dto = new UpdateMaterialDto { Id = materialId, Name = "Cotton" };

            _mockRepository.Setup(r => r.GetByIdAsync(materialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Material?)null);

            var handler = new UpdateMaterialCommandHandler(_mockRepository.Object, _mockMapper.Object);
            var command = new UpdateMaterialCommand { Material = dto };

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteMaterial_ValidId_ShouldSucceed()
        {
            var materialId = Guid.NewGuid();
            var material = OnlineShop.Domain.Entities.Material.Create("Cotton", null);

            _mockRepository.Setup(r => r.GetByIdAsync(materialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(material);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Material>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteMaterialCommandHandler(_mockRepository.Object);
            var command = new DeleteMaterialCommand(materialId);

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteMaterial_NotFound_ShouldReturnFailure()
        {
            var materialId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(materialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OnlineShop.Domain.Entities.Material?)null);

            var handler = new DeleteMaterialCommandHandler(_mockRepository.Object);
            var command = new DeleteMaterialCommand(materialId);

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}

