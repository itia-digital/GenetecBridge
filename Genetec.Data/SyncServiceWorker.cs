using Core.Data;
using Genetec.Data.Models;

namespace Genetec.Data;

public abstract class SyncServiceWorker(SyncWorker worker)
{
    protected abstract string TypeOfRecord { get; }

    protected async Task SyncAsync(DateTime startedAt, DateTime? syncedDate,
        IAsyncEnumerable<List<UpRecordValue>> fetchedRecords,
        CancellationToken cancellationToken = default)
    {
        await foreach (List<UpRecordValue> upItems in fetchedRecords
                           .WithCancellation(cancellationToken))
        {
            await worker.RunAsync(startedAt, upItems, TypeOfRecord, cancellationToken);
        }

        AlusaControl control = new()
        {
            StartedAt = startedAt,
            SyncedDate = syncedDate,
            EndedAt = DateTime.UtcNow,
            Name = nameof(ActiveEmployeesSyncService)
        };

        await worker.CreateControlAsync(control, cancellationToken);
    }
}