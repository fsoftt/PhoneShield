using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Features.RegisterAccount;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Legal;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Tests.Features.RegisterAccount;

public sealed class RegisterAccountHandlerTests
{
    private const string FirebaseUid = "firebase-uid-123";

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RegisterAccountHandler handler;

    public RegisterAccountHandlerTests()
    {
        currentUser.FirebaseUid.Returns(FirebaseUid);
        handler = new RegisterAccountHandler(currentUser, users, unitOfWork, new FakeTimeProvider(now));
    }

    [Fact]
    public async Task Handle_NewUser_CreatesAndSavesTheAccount()
    {
        var result = await handler.Handle(new RegisterAccountCommand(LegalDocuments.CurrentTermsVersion), CancellationToken.None);

        users.Received(1).Add(Arg.Is<User>(user => user.FirebaseUid == FirebaseUid && user.Id == result.Id));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.CreatedAt.Should().Be(now);
        result.AcceptedTermsVersion.Should().Be(LegalDocuments.CurrentTermsVersion);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsItWithoutCreatingAnother()
    {
        var existing = User.Register(FirebaseUid, LegalDocuments.CurrentTermsVersion, now.AddDays(-1));
        users.GetByFirebaseUidAsync(FirebaseUid, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await handler.Handle(new RegisterAccountCommand(LegalDocuments.CurrentTermsVersion), CancellationToken.None);

        result.Id.Should().Be(existing.Id);
        users.DidNotReceive().Add(Arg.Any<User>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
