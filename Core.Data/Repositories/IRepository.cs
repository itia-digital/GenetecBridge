namespace Core.Data.Repositories;

public interface IRepository
{
    IAsyncEnumerable<List<UpRecordValue>> FetchAsync(int limit = 0, int chunkSize = 1000,
        DateTime? date = null, CancellationToken cancellationToken = default);
}