using Genetec.Data.Context;
using Microsoft.Extensions.Logging;
using UP.Data;

namespace Genetec.Data;

public class StatusSyncService(IUpUnitOfWork up, GenetecDbContext genetec, ILogger<StatusSyncService> logger)
{
    /// <summary>
    ///     Syncs all records statuses: active and inactive.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        var worker = new StatusSyncWorker(up, genetec);

        // Deactivate all applicable records
        var totalInactive = await worker.SyncAsync(false, 0, 10000, cancellationToken);
        logger.LogInformation("Total inactivated records: {TotalInactive}", totalInactive);

        // Activate all applicable records
        var totalActive = await worker.SyncAsync(true, 0, 10000, cancellationToken);
        logger.LogInformation("Total activated records: {TotalIActive}", totalActive);
    }
}