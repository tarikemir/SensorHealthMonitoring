using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SensorHealthMonitoring.Server.Forms;
using SensorHealthMonitoring.Shared.Interfaces;
using SensorHealthMonitoring.Shared.Services;
using Serilog;

namespace SensorHealthMonitoring.Server;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var logger = new AppLogger("SensorHealthMonitoring.Server");

        try
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IAppLogger>(logger);
                    services.AddSingleton<IDataSerializer, MessagePackSerializer>();
                    services.AddSingleton<ITcpServerWrapper, TcpServerWrapper>();

                    services.AddSingleton<MainForm>();
                })
                .Build();

            var mainForm = host.Services.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}