using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;

namespace AnthologySap.Repositories;

public class ActiveStudentsRepository(AppDbContext context)
    : Repository(context: context),
        IActiveStudentsRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
    {
        return base.Query().Where(e => e.StatusField == "AC");
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