using Android.App;
using Android.App.Job;
using Tranqui.App.Core.Sync;

namespace Tranqui.App.Sync;

/// <summary>Runs when Android sees a network connection: sends pending reports, withdrawals and blocks.</summary>
[Service(Exported = true, Permission = "android.permission.BIND_JOB_SERVICE")]
public sealed class OutboxJobService : JobService
{
    private CancellationTokenSource? cancellation;

    public override bool OnStartJob(JobParameters? @params)
    {
        var outbox = IPlatformApplication.Current?.Services.GetService<Outbox>();
        if (outbox is null)
        {
            return false;
        }

        cancellation = new CancellationTokenSource();
        _ = RunAsync(outbox, @params, cancellation.Token);

        return true;
    }

    public override bool OnStopJob(JobParameters? @params)
    {
        cancellation?.Cancel();

        // Ask Android to run it again later.
        return true;
    }

    private async Task RunAsync(Outbox outbox, JobParameters? parameters, CancellationToken cancellationToken)
    {
        var done = false;
        try
        {
            done = await outbox.FlushAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Stopped by Android; OnStopJob already asked for a retry.
        }
        finally
        {
            JobFinished(parameters, wantsReschedule: !done && !cancellationToken.IsCancellationRequested);
        }
    }
}
