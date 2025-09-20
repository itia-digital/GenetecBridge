using System.Globalization;
using Genetec.Data.Context;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace GenetecPhotoSyncConsole;

class Program
{
    static async Task Main(string[] args)
    {
        // ✅ Create a Logger Factory
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDir);
            var now = DateTime.Now;
            var week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var fileName = $"{now:yyyy}{now:MM}-W{week:D2}-photo.log";
            string logFilePath = Path.Combine(logsDir, fileName);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: logFilePath,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            builder.AddConsole();
            builder.AddSerilog();
            builder.SetMinimumLevel(LogLevel.Information); // Set log level
        });
        ILogger<CardholderImageSyncService> logger =
            loggerFactory.CreateLogger<CardholderImageSyncService>();

        const string imageDirectory =
            "C:/Users/dproveedoralusa/Downloads/fotos_2025_04_10/PERSONAL";
        
        // const string upId = "0000988";
        // const string upId = "0282996";

        var db = new GenetecDbContext();
        var service = new CardholderImageSyncService(db, logger);

        int successCount = await service
            .ProcessDirectoryImagesAsync(imageDirectory, overwrite: true);

        Console.WriteLine(successCount > 0
            ? "Image attached successfully."
            : "Image attachment skipped or failed."
        );
    }
}