using System.Globalization;
using Genetec.Data;
using Genetec.Data.Context;
using Microsoft.Extensions.Logging;
using Serilog;
using UP.Data;
using UP.Data.Context;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GenetecSyncConsole;

class Program
{
    static async Task Main(string[] args)
    {
        // ✅ Setup Serilog
        string path = AppDomain.CurrentDomain.BaseDirectory;
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File($"{path}\\_{DateTime.Today:yyyy-MM-dd}.log",  // Logs append daily
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

        // ✅ Check for status update flag
        if (args.Any(a => a.Equals("--update-status", StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogInformation("--update-status flag detected. Running status synchronization...");
            var statusLogger = loggerFactory.CreateLogger<StatusSyncService>();
            await using var upDb = new UpDbContext();
            await using var up = new UpUnitOfWork(upDb);
            await using var genetecDb = new GenetecDbContext();
            var statusService = new StatusSyncService(up, genetecDb, statusLogger);
            await statusService.SyncAsync(cancellationTokenSource.Token);
            // Flush logs
            await Log.CloseAndFlushAsync();
            return;
        }

        // ✅ Create sync service and worker for regular sync
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
            if (args.First().StartsWith("--since=", StringComparison.OrdinalIgnoreCase))
            {
                //   ✅ Since date: Try to parse the date
                var sinceDateString = args.First()["--since=".Length..];
                if (DateTime.TryParseExact(sinceDateString,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var sinceDate))
                {
                    logger.LogInformation("Running sync from: {Date} to present", sinceDate);
                    var dateList = Enumerable.Range(0, (DateTime.Today - sinceDate).Days + 1)
                        .Select(offset => sinceDate.AddDays(offset))
                        .ToList();

                    foreach (var d in dateList)
                    {
                        logger.LogInformation("Syncing {Date}..", sinceDate);
                        await worker.SyncAsync(d.Date, cancellationTokenSource.Token);
                    }
                }
                else { logger.LogError("Invalid date form for --since param. Please use yyyy-MM-dd"); }
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
        }
        
        // Flush logs
        await Log.CloseAndFlushAsync();
    }
}