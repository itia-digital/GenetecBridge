using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using AnthologySap.Extension;

namespace AnthologySap.Repositories;

public class InactiveStudentsRepository(AppDbContext context)
    : Repository(context),
        IInactiveStudentsRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        return base.Query()
            .WhereStudentTypes()
            .WhereInactiveStudents()
            .WhereNoActiveProfile(Context);
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecStudentGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}