using Core.Data;
using UP.Data;

namespace Genetec.Data;

public class ActiveEmployeesSyncService(SyncService service, IUpUnitOfWork unitOfWork)
{
    public async Task SyncAsync(CancellationToken cancellationToken)
    {
        IAsyncEnumerable<List<UpRecordValue>> fetchedRecords = unitOfWork.ActiveEmployees
            .FetchAllRecordsInChunksAsync(cancellationToken: cancellationToken);
        
        await foreach (List<UpRecordValue> upItems in fetchedRecords)
        {
            await service.SyncAsync(upItems, cancellationToken);
        }
    }
}