using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SensorHealthMonitoring.Client;
using SensorHealthMonitoring.Client.Services;
using System.Threading;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddClientServices(context.Configuration);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddFile(context.Configuration.GetSection("Logging"));
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var client = host.Services.GetRequiredService<TcpClientManager>();
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            client.Stop();
            logger.LogInformation("Shutdown signal received");
        };

        Task? clientTask = null;

        bool isRunning = true;
        while (isRunning && !cts.Token.IsCancellationRequested)
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("1. Start - Start the client");
            Console.WriteLine("2. Stop - Stop the client");
            Console.WriteLine("3. Status - Check client status");
            Console.WriteLine("4. Exit - Exit the application");
            Console.Write("\nEnter command: ");

            string? command = Console.ReadLine()?.ToLower();

            switch (command)
            {
                case "1":
                case "start":
                    if (clientTask == null || clientTask.IsCompleted)
                    {
                        logger.LogInformation("Starting client...");
                        clientTask = Task.Run(() => client.StartAsync(), cts.Token);
                    }
                    else
                    {
                        Console.WriteLine("Client is already running.");
                    }
                    break;

                case "2":
                case "stop":
                    logger.LogInformation("Stopping client...");
                    client.Stop();
                    logger.LogInformation("Client stopped");
                    break;


                case "3":
                case "status":
                    var status = client.IsConnected ? "Connected" : "Disconnected";
                    var running = (clientTask != null && !clientTask.IsCompleted) ? "Running" : "Stopped";
                    Console.WriteLine($"Client Status: {status}");
                    Console.WriteLine($"Client Task: {running}");
                    break;

                case "4":
                case "exit":
                    isRunning = false;
                    client.Stop();
                    logger.LogInformation("Application shutting down...");
                    break;

                default:
                    Console.WriteLine("Invalid command");
                    break;
            }
        }

        if (clientTask != null && !clientTask.IsCompleted)
        {
            try
            {
                await clientTask;
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Client task cancelled");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Client task ended with error");
            }
        }
    }
}
