﻿namespace Core.Data.Extensions;
using Microsoft.EntityFrameworkCore;

public static class IQueryableExtension
{
    public static async IAsyncEnumerable<List<T>> FetchAllRecordsInChunksAsync<T>(
        this IQueryable<T> query, int chunkSize = 1000)
    {
        int offset = 0;
        bool hasMoreData;

        do
        {
            List<T> chunk = await query
                .Skip(offset)
                .Take(chunkSize)
                .ToListAsync();

            hasMoreData = chunk.Count == chunkSize;
            offset += chunkSize;

            if (chunk.Count != 0)
            {
                yield return chunk; // Stream the chunk
            }
        } while (hasMoreData);
    }
}