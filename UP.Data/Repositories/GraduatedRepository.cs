using Core.Data;
using Core.Data.Repositories;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public class GraduatedRepository(AppDbContext context)
    : Repository(context: context), IGraduatedRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        return base
            .Query()
            .Where(e =>
                (e.StatusField == "DM" && e.ProgReason == "EGR")
                || (e.StatusField == "CM" && e.ProgReason == "CRED")
                || (e.StatusField == "SP" && e.ProgReason == "EGR")
                || (e.StatusField == "SP" && e.ProgReason == "EGRP")
            );
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecGraduatedGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}