using System.Net.Sockets;

namespace SensorHealthMonitoring.Shared.Interfaces;

public interface ITcpClientWrapper : IDisposable
{
    Task ConnectAsync(string host, int port);
    NetworkStream GetStream();
    bool IsConnected { get; }
}
