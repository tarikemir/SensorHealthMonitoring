using SensorHealthMonitoring.Shared.Models;

namespace SensorHealthMonitoring.Shared.Interfaces;

public interface IMessageHandler
{
    void HandleMessage(MessagePacket<Sensor> message);
}
