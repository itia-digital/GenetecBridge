namespace Core.Data.Repositories;

public interface IStatusRepository
{
    IAsyncEnumerable<List<string>> FetchAsync(
        bool active, int limit = 0, int chunkSize = 10000,
        CancellationToken cancellationToken = default
    );
}