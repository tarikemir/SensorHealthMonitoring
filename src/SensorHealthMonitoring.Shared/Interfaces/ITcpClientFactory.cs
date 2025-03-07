using System.Net.Sockets;

namespace SensorHealthMonitoring.Shared.Interfaces;

public interface ITcpClientFactory
{
    TcpClient Create();
}
