namespace SensorHealthMonitoring.Shared.Interfaces;

public interface ITcpServerWrapper
{
    void Start(IMessageHandler handler);
    void Stop();
}
