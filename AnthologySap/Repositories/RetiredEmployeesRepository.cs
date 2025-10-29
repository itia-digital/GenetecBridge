using AnthologySap.Extension;
using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;

namespace AnthologySap.Repositories;

public class RetiredEmployeesRepository(AppDbContext context)
    : Repository(context), IRetiredRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        return base
            .Query()
            .WhereRetiredType()
            .WhereActiveCollaborator();
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