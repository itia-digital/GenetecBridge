using Core.Data;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public class InactiveStudentsRepository(AppDbContext context)
    : Repository(context: context),
        IInactiveStudentsRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        string[] statuses = ["CN", "DC", "DE", "LA"];
        return base
            .Query()
            .Where(e => EF.Constant(statuses).Contains(e.StatusField));
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