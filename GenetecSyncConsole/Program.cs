using System.Globalization;
using Genetec.Data;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GenetecSyncConsole;

class Program
{
    static async Task Main(string[] args)
    {
        // ✅ Setup Serilog
        string path = AppDomain.CurrentDomain.BaseDirectory;
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File($"{path}\\_{DateTime.Now:yyyy-MM-ddTh-mm-ss}.log",  // Logs append daily
                rollingInterval: RollingInterval.Day,  // Rolls over by day
                retainedFileCountLimit: 7,  // Keep logs for the last 7 days (optional)
                shared: true)  // Allows multiple processes to write to the log
            .CreateLogger();

        // ✅ Create a Logger Factory
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog();
            builder.AddConsole();
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
            await worker.SyncAsync(DateTime.Today, cancellationTokenSource.Token);
        }
        else
        {
            //   ✅ By date: Try to parse the date
            if (DateTime.TryParseExact(args[0],
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
            {
                logger.LogInformation("Valid date received: {Date}", parsedDate);
                await worker.SyncAsync(parsedDate, cancellationTokenSource.Token);
            }
            else { logger.LogError("Invalid date format! Please use yyyy-MM-dd"); }
        }
        
        // Flush logs
        await Log.CloseAndFlushAsync();
    }
}