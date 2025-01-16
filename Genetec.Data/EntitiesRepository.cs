using Core.Data;
using Genetec.Data.Context;
using Genetec.Data.Entities;

namespace Genetec.Data;

public interface IEntitiesRepository : IRepository<Entity>;

public class EntitiesRepository(GenetecDbContext context) 
    : Repository<GenetecDbContext, Entity>(context: context),
        IEntitiesRepository
{
    public override IQueryable<Entity> Query()
    {
        return Table; // TODO Ensure query to return Graduates only
    }
}