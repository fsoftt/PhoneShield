using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Contacts;
using Tranqui.Contracts.Accounts;
using Tranqui.Contracts.Contacts;
using Tranqui.Domain.Legal;
using Tranqui.Domain.Reputation;

namespace Tranqui.App.Core.Tests.Contacts;

public sealed class ContactContributionServiceTests
{
    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IDeviceContactSource contacts = Substitute.For<IDeviceContactSource>();
    private readonly IContributionState state = Substitute.For<IContributionState>();
    private readonly FakeTimeProvider time = new(new DateTimeOffset(2026, 10, 3, 12, 0, 0, TimeSpan.Zero));
    private readonly ContactContributionService service;

    public ContactContributionServiceTests()
    {
        api.UploadContactsAsync(Arg.Any<UploadContactsRequest>(), Arg.Any<CancellationToken>())
            .Returns(call => new UploadContactsResponse(call.Arg<UploadContactsRequest>().Contacts.Count, 0));
        service = new ContactContributionService(api, contacts, state, time);
    }

    [Fact]
    public async Task Start_RecordsConsentThenUploadsInBatches()
    {
        GivenContacts(ContactUploadRules.MaxContactsPerBatch + 1);

        var accepted = await service.StartAsync(CancellationToken.None);

        accepted.Should().Be(ContactUploadRules.MaxContactsPerBatch + 1);
        await api.Received(1).AcceptContactUploadAsync(
            new AcceptContactUploadRequest(LegalDocuments.CurrentContactUploadVersion), Arg.Any<CancellationToken>());
        await api.Received(2).UploadContactsAsync(Arg.Any<UploadContactsRequest>(), Arg.Any<CancellationToken>());
        state.Received().IsContributing = true;
        state.Received().LastSyncedAt = time.GetUtcNow();
    }

    [Fact]
    public async Task Stop_WithdrawsEverythingAndForgetsTheState()
    {
        await service.StopAsync(CancellationToken.None);

        await api.Received(1).WithdrawContactsAsync(Arg.Any<CancellationToken>());
        state.Received().IsContributing = false;
        state.Received().LastSyncedAt = null;
    }

    [Fact]
    public async Task SyncIfDue_NotContributing_DoesNothing()
    {
        await service.SyncIfDueAsync(CancellationToken.None);

        await api.DidNotReceive().UploadContactsAsync(Arg.Any<UploadContactsRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncIfDue_SyncedRecently_DoesNothing()
    {
        state.IsContributing.Returns(true);
        state.LastSyncedAt.Returns(time.GetUtcNow().AddHours(-1));
        GivenContacts(1);

        await service.SyncIfDueAsync(CancellationToken.None);

        await api.DidNotReceive().UploadContactsAsync(Arg.Any<UploadContactsRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncIfDue_AfterTheInterval_Uploads()
    {
        state.IsContributing.Returns(true);
        state.LastSyncedAt.Returns(time.GetUtcNow() - ContactContributionService.SyncInterval);
        GivenContacts(1);

        await service.SyncIfDueAsync(CancellationToken.None);

        await api.Received(1).UploadContactsAsync(Arg.Any<UploadContactsRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncIfDue_Offline_FailsSilently()
    {
        state.IsContributing.Returns(true);
        GivenContacts(1);
        api.UploadContactsAsync(Arg.Any<UploadContactsRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException());

        var act = () => service.SyncIfDueAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private void GivenContacts(int count) =>
        contacts.ReadAll().Returns(Enumerable.Range(0, count).Select(index => new DeviceContact($"300{index:D7}", "Nombre")).ToList());
}
