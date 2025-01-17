using Core.Data;
using UP.Data;

namespace Genetec.Data;

public class ActiveEmployeesSyncService(SyncService service, IUpUnitOfWork unitOfWork)
{
    /// <summary>
    /// Syncs records and set the same genetec access group
    /// to all the records
    /// </summary>
    /// <param name="limit">Set zero for no limit</param>
    /// <param name="chunkSize"></param>
    /// <param name="cancellationToken"></param>
    public async Task SyncAsync(int limit = 0, int chunkSize = 2000, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<List<UpRecordValue>> fetchedRecords = unitOfWork.ActiveEmployees
            .FetchAllRecordsInChunksAsync(limit, chunkSize, cancellationToken: cancellationToken);
        
        await foreach (List<UpRecordValue> upItems in fetchedRecords)
        {
            await service.SyncAsync(upItems, cancellationToken);
        }
    }

    /// <summary>
    /// Reads alusa records and deletes all records synced by
    /// alusa process
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await service.ResetAsync(cancellationToken);
    }

    /// <summary>
    /// Counts alusa records
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task<int> CountSyncedAsync(CancellationToken cancellationToken = default)
    {
        return await service.CountSyncedAsync(cancellationToken);
    }
}