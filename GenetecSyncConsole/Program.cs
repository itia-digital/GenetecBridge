using System.Globalization;
using Genetec.Data;
using Genetec.Data.Context;
using Microsoft.Extensions.Logging;
using Serilog;
using AnthologySap;
using AnthologySap.Models;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GenetecSyncConsole;

class Program
{
    static async Task Main(string[] args)
    {
         await RunAsync(args);
    }

    private static async Task RunAsync(string[] args)
    {
        // ✅ Create Logger
        var loggerFactory = CreateLogger();
        ILogger logger = loggerFactory.CreateLogger<Program>();

        // Emit a startup log via both pipelines for diagnostics
        logger.LogInformation("GenetecSyncConsole starting at {UtcNow}", DateTime.UtcNow);
        Log.Information("[Serilog] GenetecSyncConsole starting at {UtcNow}", DateTime.UtcNow);

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
            await HandleUpdateStatusAsync(loggerFactory, logger, cancellationTokenSource.Token);
            return;
        }

        // ✅ Check for export pictures flag
        // Supports: "--export-pictures", "--export-pictures=/path", or "--export-pictures /path"
        string? exportArg =
            args.FirstOrDefault(a => a.StartsWith("--export-pictures", StringComparison.OrdinalIgnoreCase));
        bool exportPictures = exportArg != null;
        if (!exportPictures)
        {
            // also support split form: --export-pictures <dir>
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--export-pictures", StringComparison.OrdinalIgnoreCase))
                {
                    exportPictures = true;
                    exportArg = i + 1 < args.Length ? args[i + 1] : null;
                    break;
                }
            }
        }

        if (exportPictures)
        {
            await HandleExportPicturesAsync(loggerFactory, logger, exportArg, cancellationTokenSource.Token);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        // ✅ Create sync service and worker for regular sync
        var service = new SyncService(logger);
        var worker = new Worker(service, logger);

        // ✅ Run worker
        //   ✅ By date: as today
        if (args.Length == 0)
        {
            logger.LogInformation("Running status synchronization for {Date}...", DateTime.Today.ToShortDateString());
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
                else
                {
                    logger.LogError("Invalid date form for --since param. Please use yyyy-MM-dd");
                }
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
                else
                {
                    logger.LogError("Invalid date format! Please use yyyy-MM-dd");
                }
            }
        }

        // Flush logs
        await Log.CloseAndFlushAsync();
    }

    private static ILoggerFactory CreateLogger()
    {
        // Configure Serilog to log to console and rolling weekly log files
        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsDir);
        var now = DateTime.Now;
        var week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        var logFilePath = Path.Combine(logsDir, $"{now:yyyy}{now:MM}-W{week:D2}.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: logFilePath,
                shared: true,
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        return LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            // Route Microsoft.Extensions.Logging to Serilog (which writes Console + File)
            builder.AddSerilog(Log.Logger, dispose: true);
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }

    private static async Task HandleUpdateStatusAsync(ILoggerFactory loggerFactory, ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("--update-status flag detected. Running status synchronization...");
        var statusLogger = loggerFactory.CreateLogger<StatusSyncService>();
        await using var upDb = new AppDbContext();
        await using var up = new SourceUnitOfWork(upDb);
        await using var genetecDb = new GenetecDbContext();
        var statusService = new StatusSyncService(up, genetecDb, statusLogger);
        await statusService.SyncAsync(ct);
        await Log.CloseAndFlushAsync();
    }

    private static async Task HandleExportPicturesAsync(ILoggerFactory loggerFactory, ILogger logger, string? exportArg,
        CancellationToken ct)
    {
        string? exportDir = null;
        if (exportArg != null)
        {
            var idx = exportArg.IndexOf('=');
            if (idx >= 0 && idx < exportArg.Length - 1)
            {
                exportDir = exportArg[(idx + 1)..];
            }
            else if (!exportArg.StartsWith("--export-pictures", StringComparison.OrdinalIgnoreCase))
            {
                exportDir = exportArg; // split arg form captured as exportArg
            }
        }

        var effectiveDir = string.IsNullOrWhiteSpace(exportDir)
            ? Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "exported-pictures"))
            : exportDir;

        Console.Write($"This will export all cardholder pictures to: {effectiveDir}\nProceed? [y/N] ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (response != "y")
        {
            logger.LogInformation("Export cancelled by user.");
            await Log.CloseAndFlushAsync();
            return;
        }

        logger.LogInformation("--export-pictures flag detected. Exporting pictures to directory: {Dir}", effectiveDir);
        await using var genetecDb = new GenetecDbContext();
        var exportLogger = loggerFactory.CreateLogger<PictureExportService>();
        var exportService = new PictureExportService(genetecDb, exportLogger);
        var count = await exportService.ExportCardholderPicturesAsync(effectiveDir, ct);
        logger.LogInformation("Export completed. Files written: {Count}", count);
        await Log.CloseAndFlushAsync();
    }
}