using System.Globalization;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace GenetecSyncConsole;

public static class LogSetupHelper
{
    public static ILoggerFactory CreateLogger(LogEventLevel serilogMinLevel, LogLevel msMinLevel)
    {
        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsDir);

        var now = DateTime.Now;
        var week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        var logFilePath = Path.Combine(
            logsDir,
            $"{now:yyyy}-{now:MM}-W{week:D2}-{now:dd}.log"
        );

        // Enable Serilog self-diagnostics (only printed if errors occur)
        Serilog.Debugging.SelfLog.Enable(Console.Error);

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(serilogMinLevel)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: logFilePath,
                shared: true,
                restrictedToMinimumLevel: serilogMinLevel,
                rollingInterval: RollingInterval.Infinite,
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        // Forward Microsoft.Extensions.Logging -> Serilog
        return LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: false);
            builder.SetMinimumLevel(msMinLevel);
        });
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
}
