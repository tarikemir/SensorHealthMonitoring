using MessagePack;

namespace SensorHealthMonitoring.Shared.Models;

[MessagePackObject]
public class MessagePacket<T>
{
    [Key(0)]
    public Guid Id { get; set; }
    [Key(1)]
    public T? Data { get; set; }
    [Key(2)]
    public DateTime Timestamp { get; set; }

    public static MessagePacket<T> Create(T data)
    {
        return new MessagePacket<T>
        {
            Id = Guid.NewGuid(),
            Data = data,
            Timestamp = DateTime.Now
        };
    }
}