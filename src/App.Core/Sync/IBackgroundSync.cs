namespace Tranqui.App.Core.Sync;

/// <summary>Asks the platform to run <see cref="Outbox.FlushAsync"/> once the phone has a connection, even if the app is closed.</summary>
public interface IBackgroundSync
{
    void ScheduleFlush();
}
