using System.Globalization;
using Genetec.Data;
using Genetec.Data.Context;
using Microsoft.Extensions.Logging;
using Serilog;
using Amazon;
using Amazon.Runtime;
using Amazon.CloudWatchLogs;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using UP.Data;
using UP.Data.Context;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GenetecSyncConsole;

class Program
{
    static async Task Main(string[] args)
    {
        // ✅ Set up Serilog with AWS CloudWatch
        // Read AWS config from environment variables
        var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION");
        const string logGroup = "alusa/genetec-bridge";
        const string logStreamPrefix = "up";

        if (string.IsNullOrWhiteSpace(awsRegion))
        {
            await Console.Error.WriteLineAsync("AWS_REGION (or AWS_DEFAULT_REGION) environment variable is not set. Falling back to console logging only.");
        }

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext();

        if (!string.IsNullOrWhiteSpace(awsRegion))
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
            var creds = new EnvironmentVariablesAWSCredentials();

            var cloudWatchClient = new AmazonCloudWatchLogsClient(creds, regionEndpoint);
            var streamName = $"{logStreamPrefix}-{Environment.MachineName}-{DateTime.UtcNow:yyyyMMdd}";
            var cloudWatchOptions = new CloudWatchSinkOptions
            {
                LogGroupName = logGroup,
                TextFormatter = new CompactJsonFormatter(),
                LogStreamNameProvider = new StaticLogStreamProvider(streamName),
                Period = TimeSpan.FromSeconds(5),
                BatchSizeLimit = 100,
                QueueSizeLimit = 10000,
                CreateLogGroup = true
            };

            loggerConfig = loggerConfig.WriteTo.AmazonCloudWatch(cloudWatchOptions, cloudWatchClient);
        }

        // Create the Serilog pipeline (sinks configured above, e.g., AWS CloudWatch)
        Log.Logger = loggerConfig.CreateLogger();

        // Create a Microsoft LoggerFactory that uses two providers:
        // 1) Serilog provider -> forwards Microsoft logs to the Serilog pipeline (CloudWatch)
        // 2) Console provider -> writes to the console directly
        // So ILogger below is a composite that fan-outs to both CloudWatch (via Serilog) and Console.
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole(); // also log to console via MS provider
            builder.AddSerilog(Log.Logger, dispose: true); // forward to Serilog sinks (CloudWatch)
            builder.SetMinimumLevel(LogLevel.Information); // Set the log level
        });

        // ✅ Create Logger
        ILogger logger = loggerFactory.CreateLogger<Program>();
        // Emit a startup log via both pipelines for diagnostics
        logger.LogInformation("GenetecSyncConsole starting at {UtcNow}", DateTime.UtcNow);
        Serilog.Log.Information("[Serilog] GenetecSyncConsole starting at {UtcNow}", DateTime.UtcNow);

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
            await using var upDb = new AppDbContext();
            await using var up = new SourceUnitOfWork(upDb);
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

internal sealed class StaticLogStreamProvider(string name) : ILogStreamNameProvider
{
    private readonly string _name = name ?? throw new ArgumentNullException(nameof(name));

    public string GetLogStreamName()
    {
        return _name;
    }
}
