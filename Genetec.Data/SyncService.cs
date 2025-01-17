using System.Linq.Expressions;
using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data;

public class SyncService(GenetecDbContext context)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">Chunk of data to sync</param>
    /// <param name="matching">Expression to match or create</param>
    /// <param name="whenMatched">Expression to update values when found (existingValue, newValue) => finaleValue</param>
    public async Task RunAsync<TGenetec>(
        List<TGenetec> data,
        Expression<Func<TGenetec, object>> matching,
        Expression<Func<TGenetec, TGenetec, TGenetec>> whenMatched)
        where TGenetec : class
    {
        await context.Set<TGenetec>()
            .UpsertRange(data)
            .On(matching)
            .WhenMatched(whenMatched)
            .RunAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">Chunk of data to sync</param>
    /// <param name="groupBy">Ensure uniqueness of record</param>
    /// <param name="mapRecord">Converts Up record to Genetec</param>
    /// <param name="matching">Expression to match or create</param>
    /// <param name="whenMatches">Expression to update values when found</param>
    public async Task RunAsync<TUp, TGenetec>(
        List<TUp> data,
        Func<TUp, string> groupBy,
        Func<TUp, TGenetec> mapRecord,
        Expression<Func<TGenetec, object>> matching,
        Expression<Func<TGenetec, TGenetec>> whenMatches)
        where TGenetec : class
    {
        List<TGenetec> d = Compile(data, groupBy, mapRecord);

        await context.Set<TGenetec>()
            .UpsertRange(d)
            .On(matching)
            .WhenMatched(whenMatches)
            .RunAsync();
    }

    /// <summary>
    /// Perform actions to prepare information to be inserted.
    /// </summary>
    /// <param name="source">List of items to process</param>
    /// <param name="groupBy">Ensure uniqueness of record</param>
    /// <param name="mapRecord">Converts Up record to Genetec</param>
    /// <returns></returns>
    private List<TGenetec> Compile<TUp, TGenetec>(List<TUp> source,
        Func<TUp, string> groupBy,
        Func<TUp, TGenetec> mapRecord)
    {
        List<TUp> values = RemoveDuplicated(source, groupBy).ToList();
        return values.Select(mapRecord).ToList();
    }

    /// <summary>
    /// Removes duplicated info (PK), persisting last record on list
    /// </summary>
    /// <param name="source">List of items to process</param>
    /// <param name="groupBy">Ensure uniqueness of record</param>
    /// <returns></returns>
    private IEnumerable<TUp> RemoveDuplicated<TUp>(List<TUp> source,
        Func<TUp, string> groupBy)
    {
        // Removes duplicated entries
        Dictionary<string, TUp> dictionary = new();
        source.ForEach(x => dictionary[groupBy(x)] = x);
        return dictionary.Values;
    }
}