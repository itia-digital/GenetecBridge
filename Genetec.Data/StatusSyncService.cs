using Core.Data;
using Genetec.Data.Context;
using Microsoft.Extensions.Logging;

namespace Genetec.Data;

public class StatusSyncService(ISourceUnitOfWork source, GenetecDbContext genetec, ILogger<StatusSyncService> logger)
{
    /// <summary>
    ///     Syncs all records statuses: active and inactive.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        var worker = new StatusSyncWorker(source, genetec);

        // Deactivate all applicable records
        var totalInactive = await worker.SyncAsync(false, 0, 5000, cancellationToken);
        logger.LogInformation("Total inactivated records: {TotalInactive}", totalInactive);

        // Activate all applicable records
        var totalActive = await worker.SyncAsync(true, 0, 5000, cancellationToken);
        logger.LogInformation("Total activated records: {TotalIActive}", totalActive);
    }
}