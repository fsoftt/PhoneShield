using MediatR;
using Tranqui.Application.Features.PurgeExpiredData;

namespace Tranqui.Api.BackOffice;

/// <summary>Deletes expired data once a day, starting shortly after the API boots.</summary>
internal sealed partial class DailyPurgeService(IServiceScopeFactory scopes, ILogger<DailyPurgeService> logger) : BackgroundService
{
    private static readonly TimeSpan firstRunDelay = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan interval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(firstRunDelay, stoppingToken);
            using var timer = new PeriodicTimer(interval);
            do
            {
                await PurgeAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var result = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new PurgeExpiredDataCommand(), cancellationToken);
            LogPurged(result.AppealQuotaUsages, result.ResolvedAppeals, result.SpamReports);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogPurgeFailed(exception.GetType().Name);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Purged {QuotaUsages} quota uses, {Appeals} appeals and {Reports} reports")]
    private partial void LogPurged(int quotaUsages, int appeals, int reports);

    [LoggerMessage(Level = LogLevel.Error, Message = "Daily purge failed: {ExceptionType}")]
    private partial void LogPurgeFailed(string exceptionType);
}
