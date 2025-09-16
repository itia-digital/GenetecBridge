using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genetec.Data;

/// <summary>
/// Exports cardholder pictures from the Genetec database FileCache to a directory on disk.
/// Files are saved as "{UpId}.{extension}" using the Cardholder.PictureNavigation contents.
/// </summary>
public class PictureExportService(GenetecDbContext context, ILogger<PictureExportService> logger)
{
    private readonly GenetecDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<PictureExportService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Export all cardholders' pictures to the given directory.
    /// </summary>
    /// <param name="directory">Destination directory. If null/empty, defaults to "exported-pictures" under current working directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of files successfully exported.</returns>
    public async Task<int> ExportCardholderPicturesAsync(string? directory,
        CancellationToken cancellationToken = default)
    {
        var targetDir = string.IsNullOrWhiteSpace(directory)
            ? Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "exported-pictures"))
            : Path.GetFullPath(directory!);

        Directory.CreateDirectory(targetDir);

        // Load only the data we need
        var cardholders = await _context.Cardholders
            .AsNoTracking()
            .Include(c => c.PictureNavigation)
            .Where(c => c.UpId != null && c.Picture != null && c.PictureNavigation != null)
            .Select(c => new
            {
                c.UpId,
                c.PictureNavigation!.Contents,
                c.PictureNavigation.Extension
            })
            .ToListAsync(cancellationToken);

        int success = 0;
        foreach (var ch in cardholders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(ch.UpId) || ch.Contents == null || ch.Contents.Length == 0)
            {
                continue;
            }

            var ext = SanitizeExtension(ch.Extension);
            var fileName = $"{ch.UpId}{(string.IsNullOrEmpty(ext) ? string.Empty : $".{ext}")}";
            var path = Path.Combine(targetDir, fileName);

            try
            {
                await File.WriteAllBytesAsync(path, ch.Contents, cancellationToken);
                success++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write picture file for UpId {UpId} at {Path}", ch.UpId, path);
                // Continue with other files
            }
        }

        return success;
    }

    private static string? SanitizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return null;
        var e = extension.Trim().Trim('.');
        // guard against pathological values; keep it simple (letters/digits up to 10 chars)
        e = new string(e.Where(char.IsLetterOrDigit).ToArray());
        return e.Length is not (0 or > 10)
            ? e.ToLowerInvariant()
            : null;
    }
}