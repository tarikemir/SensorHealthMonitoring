using MessagePack;
using SensorHealthMonitoring.Shared.Enums;

namespace SensorHealthMonitoring.Shared.Models;

[MessagePackObject]
public class Sensor
{
    [Key(0)]
    public int SensorId { get; set; }

    [Key(1)]
    public SensorHealth HealthStatus { get; set; }
}
