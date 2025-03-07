using SensorHealthMonitoring.Shared.Enums;
using SensorHealthMonitoring.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorHealthMonitoring.Shared.Services;

public class AppLogger : IAppLogger
{
    private readonly Dictionary<int, string> _sensorLogPaths;
    private readonly string _mainLogPath;
    private static readonly object _lock = new object();

    public AppLogger(string applicationName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var solutionDirectory = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\.."));

        var logsDirectory = Path.Combine(solutionDirectory, "logs");
        var sensorLogsDirectory = Path.Combine(logsDirectory, "sensors");

        Directory.CreateDirectory(logsDirectory);
        Directory.CreateDirectory(sensorLogsDirectory);

        _mainLogPath = Path.Combine(logsDirectory, $"{applicationName}.log");
        _sensorLogPaths = new Dictionary<int, string>();

        for (int i = 1; i <= 5; i++)
        {
            _sensorLogPaths[i] = Path.Combine(sensorLogsDirectory, $"sensor_{i}.log");
        }
    }

    public void LogSensorUpdate(int sensorId, SensorHealth status, DateTime timestamp)
    {
        if (_sensorLogPaths.TryGetValue(sensorId, out string? logPath))
        {
            var logMessage = $"{timestamp:yyyy-MM-dd HH:mm:ss.fff} - Status: {status}";
            WriteToFile(logPath, logMessage);

            LogInformation($"Sensor {sensorId}: {logMessage}");
        }
    }

    public void LogInformation(string message)
    {
        var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INFO] {message}";
        WriteToFile(_mainLogPath, logMessage);
    }

    public void LogError(string message, Exception? ex = null)
    {
        var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [ERROR] {message}";
        if (ex != null)
        {
            logMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
        }
        WriteToFile(_mainLogPath, logMessage);
    }

    public void LogWarning(string message)
    {
        var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [WARN] {message}";
        WriteToFile(_mainLogPath, logMessage);
    }

    private void WriteToFile(string path, string message)
    {
        lock (_lock)
        {
            try
            {
                File.AppendAllText(path, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
                Console.WriteLine($"Original message: {message}");
            }
        }
    }
}

