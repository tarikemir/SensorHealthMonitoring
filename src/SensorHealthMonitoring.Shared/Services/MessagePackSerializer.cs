using Microsoft.Extensions.Logging;
using SensorHealthMonitoring.Shared.Interfaces;
using MessagePack;

namespace SensorHealthMonitoring.Shared.Services;

public class MessagePackSerializer : IDataSerializer
{
    private readonly ILogger<MessagePackSerializer> _logger;

    public MessagePackSerializer(ILogger<MessagePackSerializer> logger)
    {
        _logger = logger;
    }

    public byte[] Serialize<T>(T data)
    {
        try
        {
            _logger.LogDebug("Serializing data of type {Type}", typeof(T).Name);
            return MessagePack.MessagePackSerializer.Serialize<T>(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize data of type {Type}", typeof(T).Name);
            throw;
        }
    }

    public T Deserialize<T>(byte[] data)
    {
        try
        {
            _logger.LogDebug("Deserializing data to type {Type}", typeof(T).Name);
            return MessagePack.MessagePackSerializer.Deserialize<T>(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize data to type {Type}", typeof(T).Name);
            throw;
        }
    }

}
