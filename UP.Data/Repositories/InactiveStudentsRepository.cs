using Core.Data;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IInactiveStudentsRepository : IRepository<PsUpCsIdProgdt>;

public class InactiveStudentsRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpCsIdProgdt>(context: context),
        IInactiveStudentsRepository
{
    public override IQueryable<PsUpCsIdProgdt> Query()
    {
        string[] statuses = ["CN', 'DC', 'DM', 'LA', 'SP"];
        return Table.Where(e => statuses.Contains(e.ProgStatus));
    }
}