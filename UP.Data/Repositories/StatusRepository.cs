using Core.Data.Extensions;
using Core.Data.Repositories;
using UP.Data.Context;

namespace UP.Data.Repositories;

public class StatusRepository(AppDbContext context) : IStatusRepository
{
    private IQueryable<string> GetActiveRecordsIds() => context.PsUpIdGralTVws
        .Where(e => !InActiveStatuses.Contains(e.StatusField))
        .Select(e => e.Emplid)
        .Distinct()
        .OrderBy(e => e)
    ;

    private static readonly string[] InActiveStatuses = ["CN", "DC", "DE", "LA", "I"];

    private IQueryable<string> GetInactiveRecordsIds() => context.PsUpIdGralTVws
        .Where(e => InActiveStatuses.Contains(e.StatusField))
        .Select(e => e.Emplid)
        .Distinct()
        .OrderBy(e => e)
    ;

    public IAsyncEnumerable<List<string>> FetchAsync(
        bool active, int limit = 0, int chunkSize = 10000,
        CancellationToken cancellationToken = default
    )
    {
        var query = !active
            ? GetInactiveRecordsIds()
            : GetActiveRecordsIds();

        return query.FetchAsync(limit, chunkSize, cancellationToken);
    }
}