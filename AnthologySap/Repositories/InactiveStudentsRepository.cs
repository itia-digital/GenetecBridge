using AnthologySap.Models;
using Core.Data;
using Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AnthologySap.Repositories;

public class InactiveStudentsRepository(AppDbContext context)
    : Repository(context: context),
        IInactiveStudentsRepository
{
    protected override IQueryable<VUsuariosUnificado> Query()
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