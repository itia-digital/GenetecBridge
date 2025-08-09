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
            string logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", $"_{DateTime.Today:yyyy-MM-dd}.errors.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    path: logFilePath,
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
            
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