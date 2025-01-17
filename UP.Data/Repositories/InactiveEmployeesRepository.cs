using Core.Data;
using Core.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

/// <summary>
/// Retired employees
/// </summary>
public interface IInactiveEmployeesRepository : IRepository;

public class InactiveEmployeesRepository(UpDbContext context)
    : Repository<UpDbContext, PsUpRhIdDeptdt>(context: context),
        IInactiveEmployeesRepository
{
    protected override IQueryable<PsUpRhIdDeptdt> Query()
    {
        string[] payGroup = ["UPAP001", "UPGP001", "UPMP001"];
        return Table.Where(e => e.HrStatus == "I"
                                && EF.Constant(payGroup).Contains(e.GpPaygroup));
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
                    GenetecGroup = Constants.GenetecInactiveEmployeeGroup,
                    Campus = md.Institution,
                    Phone = null
                });

        return query.FetchAllRecordsInChunksAsync(limit, chunkSize, cancellationToken);
    }
}