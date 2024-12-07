using Core.Data;
using UP.Data.Context;
using UP.Data.Entities;

namespace UP.Data;

public interface IGraduatesRepository : IRepository<PsUpCsIdProgvw>;

public class GraduatesRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpCsIdProgvw>(context: context),
        IGraduatesRepository
{
    public override IQueryable<PsUpCsIdProgvw> Query()
    {
        return Table; // TODO Ensure query to return Graduates only
    }
}