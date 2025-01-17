using Microsoft.EntityFrameworkCore;

namespace Core.Data;

public interface IRepository
{
    IAsyncEnumerable<List<UpRecordValue>> FetchAllRecordsInChunksAsync(int limit = 0,
        int chunkSize = 1000, CancellationToken cancellationToken = default);
}

public abstract class Repository<TContext, TEntity>(TContext context)
    where TContext : DbContext
    where TEntity : class
{
    protected readonly TContext Context = context;
    protected readonly DbSet<TEntity> Table = context.Set<TEntity>();
    protected virtual IQueryable<TEntity> Query() => Table;
}