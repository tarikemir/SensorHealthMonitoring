namespace SensorHealthMonitoring.Client.Options;

public class TcpClientOptions
{
    public int ReceiveBufferSize { get; set; } = 8192;
    public int SendBufferSize { get; set; } = 8192;
    public bool NoDelay { get; set; } = true;
    public int ReceiveTimeout { get; set; } = 30000;
    public int SendTimeout { get; set; } = 30000;
}
