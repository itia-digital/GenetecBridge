using Core.Data;
using Core.Data.Extensions;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IActiveEmployeesRepository : IRepository;

public class ActiveEmployeesRepository(UpDbContext context)
    : Repository<UpDbContext, PsUpRhIdDeptdt>(context: context),
        IActiveEmployeesRepository
{
    protected override IQueryable<PsUpRhIdDeptdt> Query()
    {
        string[] payGroup = ["UPA001", "UPC001", "UPE001", "UPG001", "UPM001"];
        return Table.Where(e => e.HrStatus == "A"
                                && payGroup.Contains(e.GpPaygroup));
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
                    Email = md.Emailid,
                    Name = md.FirstName,
                    LastName = md.LastName,
                    GenetecGroup = Constants.GenetecActiveEmployeeGroup,
                    Campus = md.Institution,
                    Phone = null
                });

        return query.FetchAllRecordsInChunksAsync(chunkSize, cancellationToken);
    }
}