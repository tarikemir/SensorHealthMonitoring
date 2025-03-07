using SensorHealthMonitoring.Shared.Enums;

namespace SensorHealthMonitoring.Shared.Interfaces;

public interface IAppLogger
{
    void LogSensorUpdate(int sensorId, SensorHealth status, DateTime timestamp);
    void LogInformation(string message);
    void LogError(string message, Exception? ex = null);
    void LogWarning(string message);
}
