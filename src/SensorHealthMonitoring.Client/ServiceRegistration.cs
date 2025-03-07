using SensorHealthMonitoring.Client.Services;
using SensorHealthMonitoring.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SensorHealthMonitoring.Client.Options;
using SensorHealthMonitoring.Shared.Services;

namespace SensorHealthMonitoring.Client;

public static class ServiceRegistration
{
    public static IServiceCollection AddClientServices( this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TcpClientOptions>(
            configuration.GetSection("TcpClient"));

        services.AddSingleton<ITcpClientFactory, TcpClientFactory>();
        services.AddSingleton<ITcpClientWrapper, TcpClientWrapper>();
        services.AddSingleton<IRandomGenerator, RandomGenerator>();
        services.AddSingleton<IDataSerializer, MessagePackSerializer>();
        services.AddSingleton<TcpClientManager>();

        return services;
    }
}
