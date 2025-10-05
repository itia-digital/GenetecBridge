using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Core.Data.Extensions;

namespace Genetec.Data;

/// <summary>
/// Exports cardholder pictures from the Genetec database FileCache to a directory on disk.
/// Files are saved as "{UpId}.{extension}" using the Cardholder.PictureNavigation contents.
/// </summary>
public class PictureExportService(GenetecDbContext context, ILogger<PictureExportService> logger)
{
    private readonly GenetecDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<PictureExportService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private record CardHolderPicture
    {
        public required string UpId { get; set; }
        public required byte[] Contents { get; set; }
        public required string Extension { get; set; }
    }

    /// <summary>
    /// Export all cardholders' pictures to the given directory.
    /// </summary>
    /// <param name="directory">Destination directory. If null/empty, defaults to "exported-pictures" under current working directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of files successfully exported.</returns>
    public async Task<int> ExportCardholderPicturesAsync(string? directory,
        CancellationToken cancellationToken = default)
    {
        return await ExportInternalAsync(directory, null, cancellationToken);
    }

    /// <summary>
    /// Export only the specified UpIds' pictures to the given directory.
    /// </summary>
    /// <param name="directory">Destination directory. If null/empty, defaults to "exported-pictures".</param>
    /// <param name="upIds">Collection of UpIds to export. If null or empty, nothing will be exported.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of files successfully exported.</returns>
    public async Task<int> ExportCardholderPicturesAsync(string? directory, IEnumerable<string> upIds,
        CancellationToken cancellationToken = default)
    {
        var set = upIds?.Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (set.Count != 0)
        {
            return await ExportInternalAsync(directory, set, cancellationToken);
        }

        _logger.LogInformation("No UpIds provided for picture export. Skipping.");
        return 0;
    }

    private async Task<int> ExportInternalAsync(string? directory, HashSet<string>? upIdFilter,
        CancellationToken cancellationToken)
    {
        var targetDir = string.IsNullOrWhiteSpace(directory)
            ? Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "exported-pictures"))
            : Path.GetFullPath(directory!);

        Directory.CreateDirectory(targetDir);

        var success = 0;
        await foreach (var chunk in FetchAsync(upIdFilter, cancellationToken))
        {
            foreach (var ch in chunk)
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
                }
            }
        }

        return success;
    }

    private IAsyncEnumerable<List<CardHolderPicture>> FetchAsync(HashSet<string>? upIdFilter,
        CancellationToken cancellationToken = default)
    {
        // Prepare the query: only the data we need
        var query = _context.Cardholders
            .AsNoTracking()
            .Include(c => c.PictureNavigation)
            .Where(c => c.UpId != null && c.Picture != null && c.PictureNavigation != null);

        if (upIdFilter != null)
        {
            query = query.Where(c => upIdFilter.Contains(c.UpId!));
        }

        var projection = query.Select(c => new CardHolderPicture
            { UpId = c.UpId!, Contents = c.PictureNavigation!.Contents, Extension = c.PictureNavigation.Extension });

        return projection.FetchAsync(chunkSize: 1000, cancellationToken: cancellationToken);
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