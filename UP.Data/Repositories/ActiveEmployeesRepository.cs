using Core.Data;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IActiveEmployeesRepository : IRepository;

public class ActiveEmployeesRepository(UpDbContext context)
    : Repository(context: context), IActiveEmployeesRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        string[] payGroup = ["UPA001", "UPC001", "UPE001", "UPG001", "UPM001"];
        return base
            .Query()
            .Where(e => e.StatusField == "A"
                        && EF.Constant(payGroup).Contains(e.GpPaygroup));
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecActiveEmployeeGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}