using System.Globalization;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using System.Text.RegularExpressions;
using Serilog.Formatting.Display;

namespace GenetecSyncConsole;

public static class LogSetupHelper
{
    public static async Task<ILoggerFactory> CreateLoggerAsync(LogEventLevel serilogMinLevel, LogLevel msMinLevel,
        CancellationToken cancellationToken)
    {
        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsDir);

        var now = DateTime.Now;
        var week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        var logFilePath = Path.Combine(
            logsDir,
            $"{now:yyyy}-W{week:D2}-{now:MM}-{now:dd}.log"
        );

        // Enable Serilog self-diagnostics (only printed if errors occur)
        Serilog.Debugging.SelfLog.Enable(Console.Error);

        // output template
        var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        // Configure Serilog
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(serilogMinLevel)
            .Enrich.FromLogContext()
            .WriteTo.Console(restrictedToMinimumLevel: serilogMinLevel)
            .WriteTo.File(
                path: logFilePath,
                shared: true,
                restrictedToMinimumLevel: serilogMinLevel,
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: outputTemplate
            );

        // Optional: Add AWS CloudWatch Logs sink when env vars are present
        await TryConfigureCloudWatchAsync(loggerConfig, serilogMinLevel, outputTemplate, cancellationToken);

        Log.Logger = loggerConfig.CreateLogger();

        // Forward Microsoft.Extensions.Logging -> Serilog
        return LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: false);
            builder.SetMinimumLevel(msMinLevel);
        });
    }

    private static async Task TryConfigureCloudWatchAsync(LoggerConfiguration loggerConfig,
        LogEventLevel serilogMinLevel, string outputTemplate,
        CancellationToken cancellationToken)
    {
        try
        {
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var region = Environment.GetEnvironmentVariable("AWS_REGION");

            if (string.IsNullOrWhiteSpace(region))
            {
                // Region is mandatory to target CloudWatch
                return;
            }

            // If keys are not provided, the AWS SDK will still try other credential providers (EC2/ECS/SSO, etc.)
            AWSCredentials? creds = null;
            if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
            {
                creds = new BasicAWSCredentials(accessKey, secretKey);
            }

            var regionEndpoint = RegionEndpoint.GetBySystemName(region);
            var client = creds == null
                ? new AmazonCloudWatchLogsClient(regionEndpoint)
                : new AmazonCloudWatchLogsClient(creds, regionEndpoint);

            // 🚀 Ensure log-group exists BEFORE hooking Serilog
            var logGroupName = $"ALUSA-Genetec-UP-{GetSafeMachineName()}";
            await EnsureLogGroupExistsAsync(client, logGroupName, cancellationToken);

            var options = new CloudWatchSinkOptions
            {
                CreateLogGroup = false,
                LogGroupName = logGroupName,
                Period = TimeSpan.FromSeconds(2),
                MinimumLogEventLevel = serilogMinLevel,
                TextFormatter = new MessageTemplateTextFormatter(outputTemplate),
                LogStreamNameProvider = new DefaultLogStreamProvider()
            };

            loggerConfig.WriteTo.AmazonCloudWatch(options, client);
        }
        catch (Exception ex)
        {
            // Fall back silently to console/file if CWL setup fails
            Serilog.Debugging.SelfLog.WriteLine($"CloudWatch configuration failed: {ex}");
        }
    }

    private static string GetSafeMachineName()
    {
        try
        {
            var name = Environment.MachineName;
            if (string.IsNullOrWhiteSpace(name))
                return "unknown-host";

            // CloudWatch log group names commonly allow letters, numbers, '/', '-', '_', and '.'
            // Replace anything else with '-'
            var safe = Regex.Replace(name, "[^A-Za-z0-9._-]", "-");
            return string.IsNullOrWhiteSpace(safe) ? "unknown-host" : safe;
        }
        catch
        {
            return "unknown-host";
        }
    }

    public static (LogEventLevel serilogLevel, LogLevel msLevel) ParseLogLevel(string[] args)
    {
        // Default to Information unless overridden by --verbosity
        string? levelArg = null;
        for (int i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (a.StartsWith("--verbosity", StringComparison.OrdinalIgnoreCase))
            {
                var eqIdx = a.IndexOf('=');
                if (eqIdx > 0 && eqIdx < a.Length - 1)
                {
                    levelArg = a[(eqIdx + 1)..];
                }
                else if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                {
                    levelArg = args[i + 1];
                }

                break;
            }
        }

        return !TryMapLevel(levelArg, out var serilogLevel, out var msLevel)
            ? (LogEventLevel.Information, LogLevel.Information)
            : (serilogLevel, msLevel);
    }

    private static bool TryMapLevel(string? value, out LogEventLevel serilogLevel, out LogLevel msLevel)
    {
        serilogLevel = LogEventLevel.Information;
        msLevel = LogLevel.Information;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        switch (value.Trim().ToLowerInvariant())
        {
            case "trace":
            case "verbose":
            case "v":
                serilogLevel = LogEventLevel.Verbose;
                msLevel = LogLevel.Trace;
                return true;
            case "debug":
            case "d":
                serilogLevel = LogEventLevel.Debug;
                msLevel = LogLevel.Debug;
                return true;
            case "information":
            case "info":
            case "i":
                serilogLevel = LogEventLevel.Information;
                msLevel = LogLevel.Information;
                return true;
            case "warning":
            case "warn":
            case "w":
                serilogLevel = LogEventLevel.Warning;
                msLevel = LogLevel.Warning;
                return true;
            case "error":
            case "e":
                serilogLevel = LogEventLevel.Error;
                msLevel = LogLevel.Error;
                return true;
            case "fatal":
            case "critical":
            case "c":
            case "f":
                serilogLevel = LogEventLevel.Fatal;
                msLevel = LogLevel.Critical;
                return true;
            default:
                return false;
        }
    }

    private static async Task EnsureLogGroupExistsAsync(
        IAmazonCloudWatchLogs client, string logGroupName,
        CancellationToken cancellationToken)
    {
        var attempts = 0;

        while (attempts < 10)
        {
            attempts++;

            try
            {
                var response = await client.DescribeLogGroupsAsync(
                    new Amazon.CloudWatchLogs.Model.DescribeLogGroupsRequest
                    {
                        LogGroupNamePrefix = logGroupName
                    }, cancellationToken);

                if (response.LogGroups.Any(g => g.LogGroupName == logGroupName))
                {
                    // Found — we’re ready
                    return;
                }

                // Not found — create it
                await client.CreateLogGroupAsync(new Amazon.CloudWatchLogs.Model.CreateLogGroupRequest
                {
                    LogGroupName = logGroupName
                }, cancellationToken);

                // AWS may need time to propagate
                await Task.Delay(2000, cancellationToken);
            }
            catch
            {
                // Wait a bit and retry
                await Task.Delay(1000, cancellationToken);
            }
        }

        throw new Exception($"CloudWatch Log Group '{logGroupName}' was not created after {attempts} attempts.");
    }
}