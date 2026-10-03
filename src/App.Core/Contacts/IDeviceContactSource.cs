namespace Tranqui.App.Core.Contacts;

public interface IDeviceContactSource
{
    IReadOnlyList<DeviceContact> ReadAll();
}
