using System.Globalization;
using Genetec.Data;
using Microsoft.Extensions.Logging;

namespace GenetecSyncConsole;

class Program
{
    static async Task Main(string[] args)
    {
        // ✅ Create a Logger Factory
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // Logs to console
            builder.SetMinimumLevel(LogLevel.Information); // Set log level
        });

        // ✅ Create Logger
        ILogger logger = loggerFactory.CreateLogger<Program>();
        
        var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            logger.LogWarning("CTRL+C pressed. Cancelling...");
            cancellationTokenSource.Cancel();
            eventArgs.Cancel = true; // Prevents immediate termination
        };

        // ✅ Create sync service and worker
        var service = new SyncService(logger);
        var worker = new Worker(service, logger);
        
        // ✅ Run worker
        //   ✅ By date: as today
        if (args.Length == 0)
        {
            await worker.SyncAsync(DateTime.Today, true, cancellationTokenSource.Token);
            return;
        }
        
        //   ✅ By date: Try to parse the date
        if (DateTime.TryParseExact(args[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            logger.LogInformation("Valid date received: {Date}", parsedDate);
            await worker.SyncAsync(parsedDate, true, cancellationTokenSource.Token);
        }
        else
        {
            logger.LogError("Invalid date format! Please use yyyy-MM-dd");
        }
    }
}