using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Core.Data.Extensions;

public static class QueryableExtension
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="data">Chunk of data to sync</param>
    /// <param name="distinctFn"></param>
    /// <param name="matching">Expression to match or create</param>
    /// <param name="whenMatched">Expression to update values when found (existingValue, newValue) => finaleValue</param>
    /// <param name="cancellationToken"></param>
    public static async Task ExecUpsertAsync<TContext, TGenetec>(
        this TContext context,
        List<TGenetec> data,
        Func<TGenetec, string> distinctFn,
        Expression<Func<TGenetec, object>> matching,
        Expression<Func<TGenetec, TGenetec, TGenetec>> whenMatched,
        CancellationToken cancellationToken)
        where TContext : DbContext
        where TGenetec : class
    {
        var src = data.RemoveDuplicated(distinctFn);

        await context.Set<TGenetec>()
            .UpsertRange(src)
            .On(matching)
            .WhenMatched(whenMatched)
            .RunAsync(cancellationToken);
    }

    /// <summary>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="limit">Set zero for no limit</param>
    /// <param name="chunkSize"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async IAsyncEnumerable<List<T>> FetchAsync<T>(
        this IQueryable<T> query, int limit = 0, int chunkSize = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var offset = 0;
        bool hasMoreData;

        if (limit != 0 && chunkSize > limit)
            throw new ArgumentException(
                $"{nameof(chunkSize)} must be less than or equal to {nameof(limit)}.",
                nameof(chunkSize));

        do
        {
            List<T> chunk = await query
                .Skip(offset)
                .Take(chunkSize)
                .ToListAsync(cancellationToken);

            offset += chunkSize;
            hasMoreData = chunk.Count == chunkSize && (limit == 0 || offset < limit);

            if (chunk.Count != 0) yield return chunk; // Stream the chunk
        } while (hasMoreData);
    }

    public static IQueryable<T> ConditionalWhere<T>(
        this IQueryable<T> source, bool condition,
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, bool>>? elsePredicate = null
    ) => condition
        ? source.Where(predicate)
        : elsePredicate != null
            ? source.Where(elsePredicate)
            : source;
}