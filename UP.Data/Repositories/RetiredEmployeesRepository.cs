using Core.Data;
using Microsoft.EntityFrameworkCore;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

/// <summary>
///     Retired employees
/// </summary>
public interface IRetiredEmployeesRepository : IRepository;

public class RetiredEmployeesRepository(UpDbContext context)
    : Repository(context: context),
        IRetiredEmployeesRepository
{
    protected override IQueryable<PsUpIdGralTVw> Query()
    {
        string[] payGroup = ["UPAP001", "UPGP001", "UPMP001"];
        return base
            .Query()
            .Where(e => e.StatusField == "I"
                        && EF.Constant(payGroup).Contains(e.GpPaygroup));
    }

    public IAsyncEnumerable<List<UpRecordValue>> FetchAsync(
        int limit = 0, int chunkSize = 1000, DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        return base.FetchAsync(
            Constants.GenetecRetiredEmployeeGroup,
            limit,
            chunkSize,
            date,
            cancellationToken);
    }
}