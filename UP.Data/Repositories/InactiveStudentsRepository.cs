using Core.Data;
using Core.Data.Extensions;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IInactiveStudentsRepository : IRepository;

public class InactiveStudentsRepository(UpDbContext context)
    : Repository<UpDbContext, PsUpCsIdProgdt>(context: context),
        IInactiveStudentsRepository
{
    protected override IQueryable<PsUpCsIdProgdt> Query()
    {
        string[] statuses = ["CN', 'DC', 'DM', 'LA', 'SP"];
        return Table.Where(e => statuses.Contains(e.ProgStatus));
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAllRecordsInChunksAsync(
        int chunkSize = 1000, CancellationToken cancellationToken = default)
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
                    GenetecGroup = Constants.GenetecInactiveStudentGroup,
                    Campus = md.Institution,
                    Phone = null
                });

        return query.FetchAllRecordsInChunksAsync(chunkSize, cancellationToken);
    }
}