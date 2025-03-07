using Microsoft.Extensions.Logging;
using SensorHealthMonitoring.Shared.Interfaces;
using System.Net.Sockets;

namespace SensorHealthMonitoring.Client.Services;

public class TcpClientWrapper : ITcpClientWrapper
{
    private TcpClient _client;
    private readonly ITcpClientFactory _tcpClientFactory; 
    private readonly ILogger<TcpClientWrapper> _logger;
    private bool _disposed;

    public TcpClientWrapper(ITcpClientFactory tcpClientFactory, ILogger<TcpClientWrapper> logger)
    {
        _tcpClientFactory = tcpClientFactory ?? throw new ArgumentNullException(nameof(tcpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = _tcpClientFactory.Create();
    }

    public bool IsConnected => _client?.Connected ?? false;

    public async Task ConnectAsync(string host, int port)
    {
        try
        {
            if (_client == null || !_client.Connected)
            {
                _client?.Dispose();
                _client = _tcpClientFactory.Create();
            }

            _logger.LogInformation("Attempting to connect to {Host}:{Port}", host, port);
            await _client.ConnectAsync(host, port);
            _logger.LogInformation("Successfully connected to {Host}:{Port}", host, port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to {Host}:{Port}", host, port);
            throw;
        }
    }

    public NetworkStream GetStream()
    {
        try
        {
            if (_client == null || !_client.Connected)
            {
                _logger.LogError("Attempted to get stream while client is not connected");
                throw new InvalidOperationException("Client is not connected");
            }
            return _client.GetStream();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get network stream");
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _client?.Dispose();
            _client = null;
            _logger.LogInformation("TcpClient disposed");
        }

        _disposed = true;
    }
}