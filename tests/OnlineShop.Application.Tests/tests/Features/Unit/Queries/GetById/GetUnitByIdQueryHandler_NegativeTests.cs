using FluentAssertions;
using Moq;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Features.Unit.Queries.GetById;
using Xunit;

namespace OnlineShop.Application.Tests.Features.Unit.Queries.GetById
{
    public class GetUnitByIdQueryHandler_NegativeTests
    {
        [Fact]
        public async Task Should_Return_NotFound_When_Unit_Does_Not_Exist()
        {
            var repo = new Mock<IUnitRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Unit?)null);

            var handler = new GetUnitByIdQueryHandler(repo.Object);
            var result = await handler.Handle(new GetUnitByIdQuery { Id = Guid.NewGuid() }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }
    }
}


