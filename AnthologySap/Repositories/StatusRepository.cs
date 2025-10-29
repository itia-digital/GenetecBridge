using AnthologySap.Extension;
using AnthologySap.Models;
using Core.Data.Extensions;
using Core.Data.Repositories;

namespace AnthologySap.Repositories;

public class StatusRepository(AppDbContext context) : IStatusRepository
{
    private IQueryable<string> GetActiveRecordsIds() => context.VUsuariosUnificados
        .WhereWithActiveProfile(context)
        .Select(e => e.Emplid!)
        .Distinct()
        .OrderBy(e => e)
    ;

    private static readonly string[] InActiveStatuses = ["I", "DROP"];

    private IQueryable<string> GetInactiveRecordsIds() => context.VUsuariosUnificados
        .WhereNoActiveProfile(context)
        .Select(e => e.Emplid!)
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