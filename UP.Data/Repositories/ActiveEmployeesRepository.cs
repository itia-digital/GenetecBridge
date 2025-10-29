using Core.Data;
using Core.Data.Repositories;
using UP.Data.Context;
using UP.Data.Models;
using UP.Data.Extension;

namespace UP.Data.Repositories;

public class ActiveEmployeesRepository(AppDbContext context)
    : Repository(context), IActiveEmployeesRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        return base
                .Query()
                .WhereEmployeePayGroup()
                .WhereActiveCollaborator()
            ;
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecEmployeeGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}