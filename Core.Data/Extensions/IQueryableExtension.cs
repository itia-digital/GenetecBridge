using System.Runtime.CompilerServices;

namespace Core.Data.Extensions;

using Microsoft.EntityFrameworkCore;

public static class QueryableExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="limit">Set zero for no limit</param>
    /// <param name="chunkSize"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async IAsyncEnumerable<List<T>> FetchAllRecordsInChunksAsync<T>(
        this IQueryable<T> query, int limit = 0, int chunkSize = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int offset = 0;
        bool hasMoreData;

        if (chunkSize > limit)
        {
            throw new ArgumentException(
                $"{nameof(chunkSize)} must be less than or equal to {nameof(limit)}.",
                nameof(chunkSize));
        }

        do
        {
            List<T> chunk = await query
                .Skip(offset)
                .Take(chunkSize)
                .ToListAsync(cancellationToken);

            offset += chunkSize;
            hasMoreData = chunk.Count == chunkSize && (limit == 0 || offset < limit);

            if (chunk.Count != 0)
            {
                yield return chunk; // Stream the chunk
            }
        } while (hasMoreData);
    }
}