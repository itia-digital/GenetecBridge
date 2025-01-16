using Core.Data;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IActiveEmployeesRepository : IRepository<PsUpRhIdDeptdt>;

public class ActiveEmployeesRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpRhIdDeptdt>(context: context),
        IActiveEmployeesRepository
{
    public override IQueryable<PsUpRhIdDeptdt> Query()
    {
        string[] payGroup = ["UPA001", "UPC001", "UPE001", "UPG001", "UPM001"];
        return Table.Where(e => e.HrStatus == "A" 
            && payGroup.Contains(e.GpPaygroup));
    }
}