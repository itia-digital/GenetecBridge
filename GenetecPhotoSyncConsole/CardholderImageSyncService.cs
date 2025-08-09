using Genetec.Data.Context;
using Genetec.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenetecPhotoSyncConsole;

public class CardholderImageSyncService(
    GenetecDbContext context,
    ILogger<CardholderImageSyncService> logger)
{
    private readonly GenetecDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<CardholderImageSyncService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const long MaxVarbinarySize = 2_147_483_647; // varbinary(max) limit (2GB)
    private const int BatchSize = 50; // Process 50 images per batch

    private static readonly string[] SupportedExtensions = ["jpg", "jpeg", "png"];
    
    /// <summary>
    /// Reads all image files in a directory, extracts UpId from filenames, and attaches images to Cardholders.
    /// </summary>
    /// <param name="imageDirectory">The directory containing image files (e.g., "C:/Users/.../PERSONAL").</param>
    /// <param name="overwrite">Whether to overwrite existing Picture and Thumbnail images.</param>
    /// <returns>True if at least one image was successfully processed, false otherwise.</returns>
    public async Task<int> ProcessDirectoryImagesAsync(string imageDirectory, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(imageDirectory) || !Directory.Exists(imageDirectory))
        {
            _logger.LogWarning("Image directory '{Directory}' is invalid or does not exist",  imageDirectory);
            return 0;
        }

        _logger.LogInformation("Scanning directory '{Directory}' for image files",    imageDirectory);

        // Get all image files with supported extensions
        var imageFiles = Directory
            .EnumerateFiles(imageDirectory, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).TrimStart('.').ToLower()))
            .Select(file => new
            {
                FilePath = file,
                UpId = Path.GetFileNameWithoutExtension(file),
                Extension = Path.GetExtension(file).TrimStart('.').ToLower()
            })
            .ToList();

        if (!imageFiles.Any())
        {
            _logger.LogWarning("No image files with supported extensions ({Extensions}) found in directory '{Directory}'", 
                string.Join(", ", SupportedExtensions), imageDirectory);
            return 0;
        }

        _logger.LogInformation("Found {Count} image files in directory '{Directory}'",  imageFiles.Count, imageDirectory);

        int processed = 0;
        int successCount = 0;

        // Process files in batches
        for (int i = 0; i < imageFiles.Count; i += BatchSize)
        {
            var batch = imageFiles.Skip(i).Take(BatchSize).ToList();
            _logger.LogInformation("Processing batch {BatchNumber} with {BatchSize} images",    i / BatchSize + 1, batch.Count);

            foreach (var image in batch)
            {
                try
                {
                    bool success = await AttachImageToCardholderAsync(image.UpId, imageDirectory, image.Extension, overwrite);
                    if (success)
                    {
                        successCount++;
                        _logger.LogInformation("Successfully processed image '{FilePath}' for UpId '{UpId}'",  image.FilePath, image.UpId);
                    }
                    else
                    {
                        _logger.LogWarning("Skipped or failed to process image '{FilePath}' for UpId '{UpId}'",  image.FilePath, image.UpId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image '{FilePath}' for UpId '{UpId}'. Continuing with next image",  image.FilePath, image.UpId);
                }
                processed++;
            }

            _logger.LogInformation("Completed batch {BatchNumber}. Processed {Processed}/{Total} images",  i / BatchSize + 1, processed, imageFiles.Count);
        }

        _logger.LogInformation("Finished processing directory '{Directory}'. Processed {Processed} images. Success: {SuccessCount}", 
            imageDirectory, processed, successCount);
        return successCount;
    }
    

    /// <summary>
    /// Reads an image from a directory based on UpId, validates it, and attaches it to the Cardholder's Picture and Thumbnail.
    /// </summary>
    /// <param name="upId">The UpId of the Cardholder (e.g., "0282996").</param>
    /// <param name="imageDirectory">The directory containing the image (e.g., "C:/Users/.../PERSONAL").</param>
    /// <param name="extension">The file extension (e.g., jpg, png). Default is jpg.</param>
    /// <param name="overwrite">Whether to overwrite existing Picture and Thumbnail images.</param>
    /// <returns>True if the image was successfully attached or updated, false otherwise.</returns>
    private async Task<bool> AttachImageToCardholderAsync(string upId, string imageDirectory, string extension = "jpg", bool overwrite = false)
    {
        // Validate inputs
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

        if (string.IsNullOrWhiteSpace(extension) || !SupportedExtensions.Contains(extension.ToLower()))
        {
            _logger.LogWarning("Unsupported or invalid file extension '{Extension}'. Supported: {Supported}", extension, string.Join(", ", SupportedExtensions));
            return false;
        }

        extension = extension.ToLower().Replace(".",  "");

        // Find the Cardholder by UpId
        var cardholder = await _context.Cardholders
            .Include(c => c.PictureNavigation)
            .Include(c => c.ThumbnailNavigation)
            .FirstOrDefaultAsync(c => c.UpId == upId);

        if (cardholder == null)
        {
            _logger.LogWarning("Cardholder with UpId '{UpId}' not found", upId);
            return false;
        }

        // Check if Picture or Thumbnail already exist
        bool pictureExists = cardholder is { Picture: not null, PictureNavigation: not null };
        bool thumbnailExists = cardholder is { Thumbnail: not null, ThumbnailNavigation: not null };

        if (pictureExists && thumbnailExists && !overwrite)
        {
            _logger.LogInformation("Both Picture and Thumbnail exist for Cardholder with UpId '{UpId}'. Skipping update (overwrite=false)", upId);
            return false;
        }

        // Construct the image file path (UpId + . + Extension)
        string imagePath = Path.Combine(imageDirectory, $"{upId}.{extension}");

        // Check if the image file exists
        if (!File.Exists(imagePath))
        {
            _logger.LogWarning("Image file '{ImagePath}' not found for UpId '{UpId}'", imagePath, upId);
            return false;
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
            _logger.LogInformation("Read image file '{ImagePath}' ({Size} bytes) for UpId '{UpId}'", imagePath, fileInfo.Length, upId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read image file '{ImagePath}' for UpId '{UpId}'", imagePath, upId);
            return false;
        }

        bool updated = false;

        // Handle Picture
        if (!pictureExists || overwrite)
        {
            if (pictureExists && overwrite)
            {
                // Update existing FileCache
                cardholder.PictureNavigation!.Contents = imageBytes;
                cardholder.PictureNavigation.Extension = extension;
                cardholder.PictureNavigation.Context = "CardholderPictures";
                _logger.LogInformation("Overwriting Picture for Cardholder with UpId '{UpId}'", upId);
            }
            else
            {
                // Create new FileCache
                var fileCache = new FileCache
                {
                    Guid = Guid.NewGuid(),
                    Contents = imageBytes,
                    Extension = extension,
                    Context = "CardholderPictures",
                    RelatedEntity = cardholder.Guid
                };
                _context.FileCaches.Add(fileCache);
                cardholder.Picture = fileCache.Guid;
                _logger.LogInformation("Attached new image as Picture for Cardholder with UpId '{UpId}'", upId);
            }
            updated = true;
        }

        // Handle Thumbnail
        if (!thumbnailExists || overwrite)
        {
            if (thumbnailExists && overwrite)
            {
                // Update existing FileCache (if different from Picture)
                if (cardholder.Thumbnail != cardholder.Picture)
                {
                    cardholder.ThumbnailNavigation!.Contents = imageBytes;
                    cardholder.ThumbnailNavigation.Extension = extension;
                    cardholder.ThumbnailNavigation.Context = "CardholderPictures";
                }
                _logger.LogInformation("Overwriting Thumbnail for Cardholder with UpId '{UpId}'", upId);
            }
            else
            {
                // Use same FileCache as Picture if not overwriting, or create new
                cardholder.Thumbnail = pictureExists && overwrite ? Guid.NewGuid() : cardholder.Picture;
                if (cardholder.Thumbnail != cardholder.Picture)
                {
                    var fileCache = new FileCache
                    {
                        Guid = cardholder.Thumbnail!.Value,
                        Contents = imageBytes,
                        Extension = extension,
                        Context = "CardholderPictures",
                        RelatedEntity = cardholder.Guid
                    };
                    _context.FileCaches.Add(fileCache);
                }
                _logger.LogInformation("Attached image as Thumbnail for Cardholder with UpId '{UpId}'", upId);
            }
            updated = true;
        }

        if (!updated)
        {
            _logger.LogInformation("No updates needed for Cardholder with UpId '{UpId}'", upId);
            return false;
        }

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully saved image(s) for Cardholder with UpId '{UpId}' (overwrite={Overwrite})", upId, overwrite);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error while saving image for Cardholder with UpId '{UpId}'", upId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image for Cardholder with UpId '{UpId}'", upId);
            return false;
        }
    }
}