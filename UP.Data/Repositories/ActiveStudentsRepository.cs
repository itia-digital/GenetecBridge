using Core.Data;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IActiveStudentsRepository : IRepository;

public class ActiveStudentsRepository(UpDbContext context)
    : Repository(context: context),
        IActiveStudentsRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        return base.Query().Where(e => e.StatusField == "AC");
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null, 
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecActiveStudentGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}