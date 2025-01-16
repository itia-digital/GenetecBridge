using Core.Data;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

/// <summary>
/// Retired employees
/// </summary>
public interface IRetiredEmployeesRepository : IRepository<PsUpRhIdDeptdt>;

public class InactiveEmployeesRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpRhIdDeptdt>(context: context),
        IRetiredEmployeesRepository
{
    public override IQueryable<PsUpRhIdDeptdt> Query()
    {
        string[] payGroup = ["UPAP001", "UPGP001", "UPMP001"];
        return Table.Where(e => e.HrStatus == "I" 
                                && payGroup.Contains(e.GpPaygroup));
    }
}