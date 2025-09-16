using Core.Data;
using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data;

public class StatusSyncWorker(ISourceUnitOfWork source, GenetecDbContext genetec)
{
    /// <summary>
    ///     Syncs all records status (active or inactive).
    /// </summary>
    /// <param name="active">true to set Active, false to set Inactive</param>
    /// <param name="limit">Set zero for no limit</param>
    /// <param name="chunkSize">Chunk size for fetching UpIds</param>
    /// <param name="cancellationToken"></param>
    public async Task<int> SyncAsync(bool active, int limit = 0, int chunkSize = 10000,
        CancellationToken cancellationToken = default)
    {
        var fetchedRecords = source.Status
            .FetchAsync(active, limit, chunkSize, cancellationToken);

        var totalAffected = 0;
        await foreach (var source in fetchedRecords)
        {
            if (source.Count == 0)
                continue;

            // Set-based update: update Status for all Cardholders whose UpId is in the current chunk
            totalAffected += await genetec.Cardholders
                .Where(c => source.Contains(c.UpId!))
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(c => c.Status, c => active ? (byte)0 : (byte)1),
                    cancellationToken);
        }

        return totalAffected;
    }
}