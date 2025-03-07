using Microsoft.Extensions.Options;
using SensorHealthMonitoring.Client.Options;
using SensorHealthMonitoring.Shared.Interfaces;
using System.Net.Sockets;

namespace SensorHealthMonitoring.Client.Services;

public class TcpClientFactory : ITcpClientFactory
{
    private readonly TcpClientOptions _options;

    public TcpClientFactory(IOptions<TcpClientOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }
    public TcpClient Create()
    {
        var client = new TcpClient
        {
            ReceiveBufferSize = _options.ReceiveBufferSize,
            SendBufferSize = _options.SendBufferSize,
            NoDelay = _options.NoDelay,
            ReceiveTimeout = _options.ReceiveTimeout,
            SendTimeout = _options.SendTimeout
        };

        return client;
    }
}
