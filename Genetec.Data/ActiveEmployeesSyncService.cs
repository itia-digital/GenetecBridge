using Core.Data;
using UP.Data;

namespace Genetec.Data;

public class ActiveEmployeesSyncService(SyncService service, IUpUnitOfWork unitOfWork)
{
    /// <summary>
    /// 
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
}