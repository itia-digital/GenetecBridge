using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using AnthologySap.Extension;

namespace AnthologySap.Repositories;

public class InactiveEmployeesRepository(AppDbContext context)
    : Repository(context), IInactiveEmployeesRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        return base
            .Query()
            .WhereEmployeeType()
            .WhereInactiveCollaborator()
            .WhereNoActiveProfile(Context);
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecRetiredGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}