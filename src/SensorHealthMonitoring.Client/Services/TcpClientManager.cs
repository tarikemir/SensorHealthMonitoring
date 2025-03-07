using Microsoft.Extensions.Logging;
using SensorHealthMonitoring.Shared.Constants;
using SensorHealthMonitoring.Shared.Enums;
using SensorHealthMonitoring.Shared.Interfaces;
using SensorHealthMonitoring.Shared.Models;
using System.Net.Sockets;
using System.Threading;

namespace SensorHealthMonitoring.Client.Services;

public class TcpClientManager : IDisposable
{
    private readonly ITcpClientWrapper _client;
    private readonly IRandomGenerator _random;
    private readonly IDataSerializer _serializer;
    private readonly ILogger<TcpClientManager> _logger;

    private CancellationTokenSource _cancellationTokenSource;
    private NetworkStream _stream;
    private bool _isConnected;
    private bool _disposed;
    public bool IsConnected => _isConnected;
    public TcpClientManager(
        ITcpClientWrapper client,
        IRandomGenerator random,
        IDataSerializer serializer,
        ILogger<TcpClientManager> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync()
    {
        try
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            await ConnectToServerAsync();
            await StartSendingDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Error in TcpClientManager. Enter 1 to retry");

            try
            {
                await Task.Delay(5000, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation cancelled during retry delay");
            }
        }
    }

    private async Task ConnectToServerAsync()
    {
        try
        {
            await _client.ConnectAsync(NetworkConstants.SERVER_IP, NetworkConstants.PORT);
            _stream = _client.GetStream();
            _isConnected = true;
            _logger.LogInformation("Connected to server at {ServerIP}:{Port}",
                                NetworkConstants.SERVER_IP, NetworkConstants.PORT);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to server");
            throw;
        }
    }

    private async Task StartSendingDataAsync()
    {
        while (_isConnected && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                for (int sensorId = 1; sensorId <= 5; sensorId++)
                {
                    await SendSensorDataAsync(sensorId);
                }

                await Task.Delay(NetworkConstants.UPDATE_INTERVAL_MS,
                    _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Data sending cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending sensor data");
                _isConnected = false;
                throw;
            }
        }
    }

    private async Task SendSensorDataAsync(int sensorId)
    {
        try
        {
            var sensor = new Sensor
            {
                SensorId = sensorId,
                HealthStatus = GenerateRandomStatus()
            };

            var packet = MessagePacket<Sensor>.Create(sensor);

            var dataBytes = _serializer.Serialize(packet);

            var messageBytes = new byte[4 + dataBytes.Length];
            BitConverter.GetBytes(dataBytes.Length).CopyTo(messageBytes, 0);
            dataBytes.CopyTo(messageBytes, 4);

            await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            await _stream.FlushAsync();

            _logger.LogInformation(
                                "Sent data - DateTime: {Timestamp}, Sensor: {SensorId}, Status: {Status}",
                                packet.Timestamp,
                                sensor.SensorId,
                                sensor.HealthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending data for Sensor {SensorId}", sensorId);
            throw;
        }
    }

    private SensorHealth GenerateRandomStatus()
    {
        var statuses = new[] { SensorHealth.Good, SensorHealth.Unknown, SensorHealth.Bad };
        return statuses[_random.Next(statuses.Length)];
    }

    public void Stop()
    {
        try
        {
            _cancellationTokenSource.Cancel();
            _stream?.Close();
            _client?.Dispose();
            Dispose();
            _isConnected = false;
            _logger.LogInformation("TcpClientManager stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping TcpClientManager");
        }
    }


    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _stream?.Dispose();                 
        _client?.Dispose();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _client.Dispose();
                _stream?.Dispose();
                _logger.LogInformation("TcpClientManager disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during TcpClientManager disposal");
            }
        }

        _disposed = true;
    }
}
