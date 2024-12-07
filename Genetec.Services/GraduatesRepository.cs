using Core.Data;
using Genetec.Services.Context;
using Genetec.Services.Entities;

namespace Genetec.Services;

public interface IGraduatesRepository : IRepository<Entity>;

public class GraduatesRepository(GenetecDbContext context) 
    : Repository<GenetecDbContext, Entity>(context: context),
        IGraduatesRepository
{
    public override IQueryable<Entity> Query()
    {
        return Table; // TODO Ensure query to return Graduates only
    }
}