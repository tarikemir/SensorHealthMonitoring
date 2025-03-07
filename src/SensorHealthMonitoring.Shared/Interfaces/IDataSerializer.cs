namespace SensorHealthMonitoring.Shared.Interfaces;
public interface IDataSerializer
{
    byte[] Serialize<T>(T data);
    T Deserialize<T>(byte[] data);
}
