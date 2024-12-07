using Core.Data;
using UP.Data.Context;
using UP.Data.Entities;

namespace UP.Data;

public interface IStudentsRepository : IRepository<PsUpCsIdProgvw>;

public class StudentsesRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpCsIdProgvw>(context: context),
        IStudentsRepository
{
    public override IQueryable<PsUpCsIdProgvw> Query()
    {
        return Table; // TODO Ensure query to return students only
    }
}