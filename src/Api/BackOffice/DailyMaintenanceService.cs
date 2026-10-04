using MediatR;
using Tranqui.Application.Features.PurgeExpiredData;
using Tranqui.Application.Features.RecalculateReporterReputation;

namespace Tranqui.Api.BackOffice;

/// <summary>Once a day, starting shortly after the API boots: deletes expired data and re-rates reporters.</summary>
internal sealed partial class DailyMaintenanceService(IServiceScopeFactory scopes, ILogger<DailyMaintenanceService> logger)
    : BackgroundService
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
                await RunAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var purged = await sender.Send(new PurgeExpiredDataCommand(), cancellationToken);
            LogPurged(purged.AppealQuotaUsages, purged.ResolvedAppeals, purged.SpamReports);
            var rated = await sender.Send(new RecalculateReporterReputationCommand(), cancellationToken);
            LogReputationRecalculated(rated);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogMaintenanceFailed(exception.GetType().Name);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Purged {QuotaUsages} quota uses, {Appeals} appeals and {Reports} reports")]
    private partial void LogPurged(int quotaUsages, int appeals, int reports);

    [LoggerMessage(Level = LogLevel.Information, Message = "Reporter reputation recalculated: {Rated} reporters rated")]
    private partial void LogReputationRecalculated(int rated);

    [LoggerMessage(Level = LogLevel.Error, Message = "Daily maintenance failed: {ExceptionType}")]
    private partial void LogMaintenanceFailed(string exceptionType);
}
