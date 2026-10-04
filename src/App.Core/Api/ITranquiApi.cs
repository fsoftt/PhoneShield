using Refit;
using Tranqui.Contracts.Accounts;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.Blocks;
using Tranqui.Contracts.Contacts;
using Tranqui.Contracts.Lookups;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Api;

/// <summary>Typed client for the Tranqui API. Every call is authenticated by <see cref="AuthorizationHandler"/>.</summary>
public interface ITranquiApi
{
    [Post("/v1/account")]
    Task<AccountResponse> RegisterAccountAsync([Body] RegisterAccountRequest request, CancellationToken cancellationToken);

    [Delete("/v1/account")]
    Task DeleteAccountAsync(CancellationToken cancellationToken);

    [Get("/v1/account/export")]
    Task<MyDataResponse> ExportMyDataAsync(CancellationToken cancellationToken);

    [Post("/v1/account/consents/contact-upload")]
    Task AcceptContactUploadAsync([Body] AcceptContactUploadRequest request, CancellationToken cancellationToken);

    [Post("/v1/lookups")]
    Task<LookupResponse> LookupAsync([Body] LookupRequest request, CancellationToken cancellationToken);

    [Post("/v1/reports")]
    Task ReportCallAsync([Body] ReportCallRequest request, CancellationToken cancellationToken);

    [Delete("/v1/reports")]
    Task WithdrawReportAsync([Body] WithdrawReportRequest request, CancellationToken cancellationToken);

    [Post("/v1/blocks")]
    Task BlockAsync([Body] BlockRequest request, CancellationToken cancellationToken);

    [Delete("/v1/blocks")]
    Task UnblockAsync([Body] BlockRequest request, CancellationToken cancellationToken);

    [Post("/v1/contacts")]
    Task<UploadContactsResponse> UploadContactsAsync([Body] UploadContactsRequest request, CancellationToken cancellationToken);

    [Delete("/v1/contacts")]
    Task<WithdrawContactsResponse> WithdrawContactsAsync(CancellationToken cancellationToken);

    [Post("/v1/appeals/verification-requests")]
    Task<AppealVerificationResponse> RequestAppealVerificationAsync(
        [Body] AppealVerificationRequest request,
        CancellationToken cancellationToken);

    [Post("/v1/appeals")]
    Task<AppealResponse> SubmitAppealAsync([Body] SubmitAppealRequest request, CancellationToken cancellationToken);
}
