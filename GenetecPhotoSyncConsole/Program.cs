using Genetec.Data.Context;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GenetecPhotoSyncConsole;

class Program
{
    static async Task Main(string[] args)
    {
        // ✅ Create a Logger Factory
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information); // Set log level
        });
        ILogger<CardholderImageService> logger = loggerFactory.CreateLogger<CardholderImageService>();
        
        const string pathToPhoto = "C:/Users/dproveedoralusa/Downloads/fotos_2025_04_10/PERSONAL";
        const string upId = "0282996";

        var db = new GenetecDbContext();
        var service = new CardholderImageService(db, logger);
        
        bool success = await service.AttachImageToCardholderAsync(upId, pathToPhoto, overwrite: true);
        
        Console.WriteLine(success
            ? "Image attached successfully."
            : "Image attachment skipped or failed."
        );
    }
}