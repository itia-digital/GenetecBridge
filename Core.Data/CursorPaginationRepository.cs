using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Core.Data;

public abstract class CursorPaginationRepository<TEntity, TContext>(TContext context)
    where TContext : DbContext
    where TEntity : class
{
    protected virtual IQueryable<TEntity> BaseQuery()
    => context.Set<TEntity>().AsQueryable();
    
    /// <summary>
    /// Retrieves a paginated list of entities based on cursor pagination.
    /// </summary>
    /// <param name="params">The cursor query parameters.</param>
    /// <param name="sorting">Sorting configuration.</param>
    /// <returns>A task representing a list of paginated entities.</returns>
    public async Task<CursorPagedRecords<TEntity>> GetPaginatedAsync(
        CursorQueryParams @params, params SortingCursorSetup[] sorting)
    {
        IQueryable<TEntity> query = BaseQuery();
        List<TEntity> items = await query.ToListAsync(@params, sorting);
        return items.ToCursorPage(sorting);
    }
    /// <summary>
    /// Retrieves a paginated list of entities based on cursor pagination.
    /// Filter by query filter
    /// </summary>
    /// <param name="params">The cursor query parameters.</param>
    /// <param name="selector">Query filter</param>
    /// <param name="sorting">Sorting configuration.</param>
    /// <returns>A task representing a list of paginated entities.</returns>
    public async Task<CursorPagedRecords<TEntity>> GetPaginatedAsync(
        CursorQueryParams @params, Expression<Func<TEntity, bool>> selector, 
        params SortingCursorSetup[] sorting)
    {
        IQueryable<TEntity> query = context.Set<TEntity>().AsQueryable();
        List<TEntity> items = await query.Where(selector).ToListAsync(@params, sorting);
        return items.ToCursorPage(sorting);
    }

    /// <summary>
    /// Retrieves an entity by its key.
    /// </summary>
    /// <param name="keySelector">Key selector expression.</param>
    /// <returns>A task representing the entity.</returns>
    public async Task<TEntity?> GetByIdAsync(Expression<Func<TEntity, bool>> keySelector) 
        => await context.Set<TEntity>().FirstOrDefaultAsync(keySelector);
}