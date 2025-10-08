using AutoMapper;
using FluentAssertions;
using Moq;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Application.Features.Unit.Queries.GetAll;
using OnlineShop.Domain.Entities;
using Xunit;

namespace OnlineShop.Application.Tests.Features.Unit.Queries.GetAll
{
    public class GetAllUnitsQueryHandlerTests
    {
        private readonly Mock<IUnitRepository> _repoMock = new();
        private readonly IMapper _mapper;

        public GetAllUnitsQueryHandlerTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(OnlineShop.Application.AssemblyReference).Assembly));
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_No_Units()
        {
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Unit>());

            var handler = new GetAllUnitsQueryHandler(_repoMock.Object, _mapper);

            var result = await handler.Handle(new GetAllUnitsQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Should().BeEmpty();
        }

        [Fact]
        public async Task Should_Map_Units_To_Dtos()
        {
            var units = new List<Unit>
            {
                Unit.Create(1, "Kilogram", 1, 1, "Weight"),
                Unit.Create(2, "Liter", 1, 1, "Volume")
            };

            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(units);

            var handler = new GetAllUnitsQueryHandler(_repoMock.Object, _mapper);

            var result = await handler.Handle(new GetAllUnitsQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().Be(2);
            result.Data![0].Name.Should().Be("Kilogram");
            result.Data![0].Comment.Should().Be("Weight");
        }
    }
}


