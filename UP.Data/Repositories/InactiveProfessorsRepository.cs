using Core.Data;
using Core.Data.Repositories;
using UP.Data.Context;
using UP.Data.Models;
using UP.Data.Extension;

namespace UP.Data.Repositories;

public class InactiveProfessorsRepository(AppDbContext context)
    : Repository(context), IInactiveProfessorsRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        return base
                .Query()
                .WhereProfessorPayGroup()
                .WhereInactiveCollaborator()
                .WhereAnyActiveProfile(Context)
            ;
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecProfessorGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}