using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using AnthologySap.Extension;

namespace AnthologySap.Repositories;

public class ActiveEmployeesRepository(AppDbContext context)
    : Repository(context), IActiveEmployeesRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        return base
            .Query()
            .WhereEmployeeType()
            .WhereActiveCollaborator();
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