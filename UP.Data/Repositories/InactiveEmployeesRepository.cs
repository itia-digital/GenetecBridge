using Core.Data;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

/// <summary>
///     Retired employees
/// </summary>
public interface IInactiveEmployeesRepository : IRepository;

public class InactiveEmployeesRepository(UpDbContext context)
    : Repository(context: context),
        IInactiveEmployeesRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        return base.Query().Where(e => e.StatusField == "I");
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(int limit = 0,
        int chunkSize = 1000, CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecInactiveEmployeeGroup,
            limit,
            chunkSize,
            cancellationToken);
    }
}