using Core.Data;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IActiveStudentsRepository : IRepository<PsUpCsIdProgdt>;

public class ActiveStudentsRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpCsIdProgdt>(context: context),
        IActiveStudentsRepository
{
    public override IQueryable<PsUpCsIdProgdt> Query()
    {
        return Table.Where(e => e.ProgStatus == "AC");
    }
}