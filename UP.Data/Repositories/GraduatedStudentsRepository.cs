using Core.Data;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IGraduatedStudentRepository : IRepository;

public class GraduatedStudentRepository(UpDbContext context)
    : Repository(context: context),
        IGraduatedStudentRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        string[] status = ["DM-EGR", "CM-CRED", "SP-EGR", "SP-EGRP"];
        return base
            .Query()
            .Where(e => EF.Constant(status).Contains(e.StatusField));
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(int limit = 0,
        int chunkSize = 1000, CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecGraduatedGroup,
            limit,
            chunkSize,
            cancellationToken);
    }
}