using FluentValidation;
using MediatR;
using Tranqui.Application.Behaviors;

namespace Tranqui.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private const string ExpectedResponse = "handled";

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([]);

        var response = await behavior.Handle(new SampleRequest("value"), Next, CancellationToken.None);

        response.Should().Be(ExpectedResponse);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleRequestValidator()]);

        var response = await behavior.Handle(new SampleRequest("value"), Next, CancellationToken.None);

        response.Should().Be(ExpectedResponse);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ThrowsValidationExceptionWithoutCallingNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleRequestValidator()]);
        var nextCalled = false;

        var act = () => behavior.Handle(
            new SampleRequest(string.Empty),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(ExpectedResponse);
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(exception => exception.Errors.Any(error => error.PropertyName == nameof(SampleRequest.Value)));
        nextCalled.Should().BeFalse();
    }

    private static Task<string> Next(CancellationToken cancellationToken) => Task.FromResult(ExpectedResponse);

    public sealed record SampleRequest(string Value) : IRequest<string>;

    private sealed class SampleRequestValidator : AbstractValidator<SampleRequest>
    {
        public SampleRequestValidator()
        {
            RuleFor(request => request.Value).NotEmpty();
        }
    }
}
