using Microsoft.EntityFrameworkCore;

namespace Core.Data;

public interface IRepository<out T> where T : class
{
    public IQueryable<T> Query();
}

public abstract class Repository<TContext, T>(TContext context)
    where TContext : DbContext
    where T : class
{
    protected readonly TContext Context = context;
    protected readonly DbSet<T> Table = context.Set<T>();
    
    public virtual IQueryable<T> Query() => Table;
}