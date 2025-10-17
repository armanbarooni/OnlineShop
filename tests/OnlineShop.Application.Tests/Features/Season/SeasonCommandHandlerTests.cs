using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using OnlineShop.Application.Features.Season.Commands.Create;
using OnlineShop.Application.Features.Season.Commands.Update;
using OnlineShop.Application.Features.Season.Commands.Delete;
using OnlineShop.Application.DTOs.Season;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Tests.Features.Season
{
    public class SeasonCommandHandlerTests
    {
        private readonly Mock<ISeasonRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;

        public SeasonCommandHandlerTests()
        {
            _mockRepository = new Mock<ISeasonRepository>();
            _mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task CreateSeason_ValidRequest_ShouldSucceed()
        {
            var dto = new CreateSeasonDto { Name = "Spring", Code = "SPRING" };
            var season = OnlineShop.Domain.Entities.Season.Create("Spring", "SPRING");

            _mockRepository.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<OnlineShop.Domain.Entities.Season>(), It.IsAny<CancellationToken>()))
                .Returns((OnlineShop.Domain.Entities.Season s, CancellationToken ct) => Task.FromResult(s));

            _mockMapper.Setup(m => m.Map<SeasonDto>(It.IsAny<OnlineShop.Domain.Entities.Season>()))
                .Returns(new SeasonDto { Id = Guid.NewGuid(), Name = "Spring", Code = "SPRING" });

            var handler = new CreateSeasonCommandHandler(_mockRepository.Object, _mockMapper.Object);
            var command = new CreateSeasonCommand { Season = dto };

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateSeason_ValidRequest_ShouldSucceed()
        {
            var seasonId = Guid.NewGuid();
            var dto = new UpdateSeasonDto { Id = seasonId, Name = "Summer", Code = "SUMMER" };
            var existingSeason = OnlineShop.Domain.Entities.Season.Create("Spring", "SPRING");

            _mockRepository.Setup(r => r.GetByIdAsync(seasonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingSeason);

            _mockRepository.Setup(r => r.ExistsByNameAsync("Summer", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<OnlineShop.Domain.Entities.Season>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(m => m.Map<SeasonDto>(It.IsAny<OnlineShop.Domain.Entities.Season>()))
                .Returns(new SeasonDto { Id = seasonId, Name = "Summer", Code = "SUMMER" });

            var handler = new UpdateSeasonCommandHandler(_mockRepository.Object, _mockMapper.Object);
            var command = new UpdateSeasonCommand { Season = dto };

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteSeason_ValidId_ShouldSucceed()
        {
            var seasonId = Guid.NewGuid();
            var season = OnlineShop.Domain.Entities.Season.Create("Spring", "SPRING");

            _mockRepository.Setup(r => r.GetByIdAsync(seasonId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(season);
            _mockRepository.Setup(r => r.DeleteAsync(seasonId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteSeasonCommandHandler(_mockRepository.Object);
            var command = new DeleteSeasonCommand(seasonId);

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}

