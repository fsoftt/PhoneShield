namespace Tranqui.Domain.Appeals;

/// <summary>Keyed hash of a device id, so appeals can be counted per device without storing the id.</summary>
public interface IDeviceKeyProvider
{
    ReadOnlyMemory<byte> FromDeviceId(DeviceId deviceId);
}
