using Microsoft.Extensions.Logging;
using SensorHealthMonitoring.Shared.Constants;
using SensorHealthMonitoring.Shared.Interfaces;
using SensorHealthMonitoring.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;

namespace SensorHealthMonitoring.Shared.Services;

public class TcpServerWrapper : ITcpServerWrapper
{
    private readonly TcpListener _listener;
    private readonly ILogger<TcpServerWrapper> _logger;
    private readonly IDataSerializer _serializer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _isRunning;
    private bool _disposed;

    public TcpServerWrapper(
        ILogger<TcpServerWrapper> logger,
        IDataSerializer serializer)
    {
        _listener = new TcpListener(IPAddress.Any, NetworkConstants.PORT);
        _logger = logger;
        _serializer = serializer;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Start(IMessageHandler handler)
    {
        _listener.Start();
        _isRunning = true;
        _logger.LogInformation("Server started on port {Port}", NetworkConstants.PORT);

        Task.Run(() => AcceptClientsAsync(handler));
    }

    private async Task AcceptClientsAsync(IMessageHandler handler)
    {
        while (_isRunning)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client, handler);
                _logger.LogInformation("New client connected");
            }
            catch (Exception ex) when (!_isRunning)
            {
                // Normal shutdown, ignore
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting client");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, IMessageHandler handler)
    {
        try
        {
            using var stream = client.GetStream();
            var lengthBuffer = new byte[4];

            while (_isRunning)
            {
                await stream.ReadAsync(lengthBuffer, 0, 4);
                var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                var messageBuffer = new byte[messageLength];
                await stream.ReadAsync(messageBuffer, 0, messageLength);

                var message = _serializer.Deserialize<MessagePacket<Sensor>>(messageBuffer);
                message.Timestamp = message.Timestamp.AddHours(+3);
                handler.HandleMessage(message);

                _logger.LogDebug(
                    "Received data from Sensor {SensorId}: {Status}",
                    message.Data?.SensorId,
                    message.Data?.HealthStatus);
            }
        }
        catch (Exception ex) when (!_isRunning)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client");
        }
        finally
        {
            client.Dispose();
        }
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _logger.LogInformation("Server stopped");
    }

    public void Dispose()
    {
        if (_disposed) return;

        Stop();
        _cancellationTokenSource.Dispose();
        _disposed = true;
    }
}

