using Core.Data;
using UP.Data.Context;
using UP.Data.Models;

namespace UP.Data.Repositories;

public interface IGraduatedStudentRepository : IRepository<PsUpCsIdProgdt>;

public class GraduatedStudentRepository(UpDbContext context) 
    : Repository<UpDbContext, PsUpCsIdProgdt>(context: context),
        IGraduatedStudentRepository
{
    public override IQueryable<PsUpCsIdProgdt> Query()
    {
        return Table.Where(e => e.ProgStatus == "CM");
    }
}