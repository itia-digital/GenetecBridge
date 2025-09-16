using Core.Data;

namespace Genetec.Data;

public class RetiredSyncService(SyncWorker worker, ISourceUnitOfWork unitOfWork)
    : SyncServiceWorker(worker), ISyncService
{
    /// <summary>
    ///     Syncs records and set the same genetec access group
    ///     to all the records
    /// </summary>
    /// <param name="startedAt"></param>
    /// <param name="limit">Set zero for no limit</param>
    /// <param name="chunkSize"></param>
    /// <param name="date"></param>
    /// <param name="cancellationToken"></param>
    public async Task SyncAsync(DateTime startedAt, int limit = 0, int chunkSize = 2000,
        DateTime? date = null, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<List<UpRecordValue>> fetchedRecords = unitOfWork.RetiredEmployees
            .FetchAsync(limit, chunkSize, date, cancellationToken);

        await SyncAsync(startedAt, date, fetchedRecords, cancellationToken);
    }
}