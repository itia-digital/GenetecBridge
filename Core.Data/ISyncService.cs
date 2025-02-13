namespace Core.Data;

public interface ISyncService
{
    Task SyncAsync(DateTime startedAt, int limit = 0, int chunkSize = 2000,
        DateTime? date = null, CancellationToken cancellationToken = default);
}
