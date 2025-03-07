using Microsoft.Extensions.Logging;
using SensorHealthMonitoring.Shared.Interfaces;

namespace SensorHealthMonitoring.Client.Services;

public class RandomGenerator : IRandomGenerator
{
    private readonly Random _random;
    private readonly ILogger<RandomGenerator> _logger;

    public RandomGenerator(ILogger<RandomGenerator> logger)
    {
        _logger = logger;
        _random = new Random();
    }

    public int Next(int maxValue)
    {
        try
        {
            return _random.Next(maxValue);
        }
        catch (Exception ex) {

            _logger.LogError(ex, "Failed to generate random number with max value {MaxValue}", maxValue);
            throw;
        }
        
    }
}