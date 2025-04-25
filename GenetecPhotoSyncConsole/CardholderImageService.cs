using Genetec.Data.Context;
using Genetec.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenetecPhotoSyncConsole;

public class CardholderImageService(
    GenetecDbContext context,
    ILogger<CardholderImageService> logger)
{
    private readonly GenetecDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<CardholderImageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const long MaxVarbinarySize = 2_147_483_647; // varbinary(max) limit (2GB)

    /// <summary>
    /// Reads an image from a directory based on UpId, validates it, and attaches it to the Cardholder's Picture and Thumbnail.
    /// </summary>
    /// <param name="upId">The UpId of the Cardholder (e.g., "0282996").</param>
    /// <param name="imageDirectory">The directory containing the image (e.g., "C:/Users/.../PERSONAL").</param>
    /// <param name="extension">The file extension's like png/jpg default is jpg</param>
    /// <param name="overwrite"></param>
    /// <returns>True if the image was successfully attached, false otherwise.</returns>
    public async Task<bool> AttachImageToCardholderAsync(string upId, string imageDirectory, string extension = "jpg", bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(upId))
        {
            _logger.LogWarning("{UpId} is null or empty", nameof(upId));
            return false;
        }

        if (string.IsNullOrWhiteSpace(imageDirectory) || !Directory.Exists(imageDirectory))
        {
            _logger.LogWarning("Image directory '{Directory}' is invalid or does not exist", imageDirectory);
            return false;
        }

        // Find the Cardholder by UpId
        var cardholder = await _context.Cardholders
            .FirstOrDefaultAsync(c => c.UpId == upId);

        if (cardholder == null)
        {
            _logger.LogWarning("Cardholder with UpId '{UpId}' not found", upId);
            return false;
        }

        // Check if Picture or Thumbnail already exist
        bool pictureExists = cardholder.Picture.HasValue &&
                             await _context.FileCaches.AnyAsync(f => f.Guid == cardholder.Picture);
        bool thumbnailExists = cardholder.Thumbnail.HasValue &&
                               await _context.FileCaches.AnyAsync(f => f.Guid == cardholder.Thumbnail);

        if (pictureExists && thumbnailExists)
        {
            if (!overwrite)
            {
                _logger.LogInformation("Both Picture and Thumbnail already exist for Cardholder with UpId '{UpId}'. Skipping update", upId);
                return false; // Continue without overwriting
            }
            
            _logger.LogInformation("Both Picture and Thumbnail already exist for Cardholder with UpId '{UpId}'. Overwriting", upId);
        }

        // Construct the image file path (UpId + . + Extension)
        string imagePath = Path.Combine(imageDirectory, $"{upId}.{extension}");

        // Check if the image file exists
        if (!File.Exists(imagePath))
        {
            _logger.LogWarning("Image file '{ImagePath}' not found for UpId '{UpId}'", imagePath, upId);
            return false; // Continue without attaching
        }

        // Validate file size
        var fileInfo = new FileInfo(imagePath);
        if (fileInfo.Length > MaxVarbinarySize)
        {
            _logger.LogWarning("Image file '{ImagePath}' size ({Size} bytes) exceeds varbinary(max) limit ({MaxSize} bytes)",
                imagePath, fileInfo.Length, MaxVarbinarySize);
            return false;
        }

        // Read the image file
        byte[] imageBytes;
        try
        {
            imageBytes = await File.ReadAllBytesAsync(imagePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read image file '{ImagePath}' for UpId '{UpId}'", imagePath, upId);
            return false;
        }

        // Create or update FileCache record
        var fileCacheGuid = Guid.NewGuid();
        var fileCache = new FileCache
        {
            Guid = fileCacheGuid,
            Contents = imageBytes,
            Extension = extension,
            Context = "CardholderPictures",
            RelatedEntity = cardholder.Guid
        };

        // Add or update FileCache
        if (!pictureExists || !thumbnailExists)
        {
            _context.FileCaches.Add(fileCache);
        }

        // Update Cardholder's Picture and/or Thumbnail
        if (!pictureExists)
        {
            cardholder.Picture = fileCacheGuid;
            _logger.LogInformation("Attached image as Picture for Cardholder with UpId '{UpId}'", upId);
        }

        if (!thumbnailExists)
        {
            cardholder.Thumbnail = fileCacheGuid;
            _logger.LogInformation("Attached image as Thumbnail for Cardholder with UpId '{UpId}'", upId);
        }

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully saved image for Cardholder with UpId '{UpId}'", upId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image for Cardholder with UpId '{UpId}'", upId);
            return false;
        }
    }
}