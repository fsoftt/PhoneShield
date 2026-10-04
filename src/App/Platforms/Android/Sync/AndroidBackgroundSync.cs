using Android.App.Job;
using Android.Content;
using Tranqui.App.Core.Sync;

namespace Tranqui.App.Sync;

/// <summary>Schedules <see cref="OutboxJobService"/> to run as soon as the phone has a network connection.</summary>
internal sealed class AndroidBackgroundSync : IBackgroundSync
{
    private const int JobId = 4301;

    public void ScheduleFlush()
    {
        var context = global::Android.App.Application.Context;
        if (context.GetSystemService(Context.JobSchedulerService) is not JobScheduler scheduler)
        {
            return;
        }

        var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(OutboxJobService)));
        var job = new JobInfo.Builder(JobId, component)
            .SetRequiredNetworkType(NetworkType.Any)!
            .Build()!;
        scheduler.Schedule(job);
    }
}
