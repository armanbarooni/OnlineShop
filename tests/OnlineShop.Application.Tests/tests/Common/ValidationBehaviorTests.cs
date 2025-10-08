using FluentAssertions;
using FluentValidation;
using MediatR;
using OnlineShop.Application.Common.Behaviors;
using Xunit;

namespace OnlineShop.Application.Tests.Common
{
    public class ValidationBehaviorTests
    {
        private class TestRequest : IRequest<string>
        {
            public string Name { get; set; } = string.Empty;
        }

        private class TestValidator : AbstractValidator<TestRequest>
        {
            public TestValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
            }
        }

        private class NextDelegate
        {
            public static Task<string> Execute() => Task.FromResult("OK");
        }

        [Fact]
        public async Task Should_Pass_When_Request_Is_Valid()
        {
            var validators = new List<IValidator<TestRequest>> { new TestValidator() };
            var behavior = new ValidationBehavior<TestRequest, string>(validators);

            var request = new TestRequest { Name = "valid" };

            var act = async () => await behavior.Handle(request, () => NextDelegate.Execute(), CancellationToken.None);

            await act.Should().NotThrowAsync();
            (await act()).Should().Be("OK");
        }

        [Fact]
        public async Task Should_Throw_When_Request_Is_Invalid()
        {
            var validators = new List<IValidator<TestRequest>> { new TestValidator() };
            var behavior = new ValidationBehavior<TestRequest, string>(validators);

            var request = new TestRequest { Name = string.Empty };

            var act = async () => await behavior.Handle(request, () => NextDelegate.Execute(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}


