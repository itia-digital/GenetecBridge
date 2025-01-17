using Core.Data;
using Core.Data.Extensions;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IActiveStudentsRepository : IRepository;

public class ActiveStudentsRepository(UpDbContext context)
    : Repository<UpDbContext, PsUpCsIdProgdt>(context: context),
        IActiveStudentsRepository
{
    protected override IQueryable<PsUpCsIdProgdt> Query()
    {
        return Table.Where(e => e.ProgStatus == "AC");
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAllRecordsInChunksAsync(
        int limit = 0, int chunkSize = 1000, CancellationToken cancellationToken = default)
    {
        IQueryable<UpRecordValue> query = Query()
            .SelectMany(t => Context.PsUpIdGralEVws
                    .Where(e => e.Emplid == t.Emplid)
                    .DefaultIfEmpty(),
                (src, md) => new UpRecordValue
                {
                    Id = src.Emplid,
                    Name = md.FirstName,
                    LastName = md.LastName,
                    Email = md.Emailid,
                    GenetecGroup = Constants.GenetecActiveStudentGroup,
                    Campus = md.Institution,
                    Phone = null
                });

        return query.FetchAllRecordsInChunksAsync(limit, chunkSize, cancellationToken);
    }
}