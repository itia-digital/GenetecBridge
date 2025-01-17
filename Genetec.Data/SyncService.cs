using System.Linq.Expressions;
using Genetec.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data;

public abstract class SyncService<TUp, TGenetec>(GenetecDbContext context) where TGenetec : class
{
    /// <summary>
    /// Ensure uniqueness of record
    /// </summary>
    protected abstract Func<TUp, string> GroupBy { get; }

    /// <summary>
    /// Converts Up record to Genetec
    /// </summary>
    protected abstract Func<TUp, TGenetec> MapRecord { get; }

    /// <summary>
    /// Converts Up record to Genetec
    /// </summary>
    protected abstract Expression<Func<TGenetec, object>> MatchingValues { get; }

    /// <summary>
    /// Converts Up record to Genetec
    /// </summary>
    protected abstract Expression<Func<TGenetec, TGenetec>> WhenMatches { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    public async Task RunAsync(IQueryable<TUp> query)
    {
        List<TUp> data = await query.ToListAsync();
        List<TGenetec> d = Compile(data);
        
        await context.Set<TGenetec>()
            .UpsertRange(d)
            .On(MatchingValues)
            .WhenMatched(WhenMatches)
            .RunAsync();
    }

    /// <summary>
    /// Perform actions to prepare information to be inserted.
    /// </summary>
    /// <param name="source">List of items to process</param>
    /// <returns></returns>
    private List<TGenetec> Compile(List<TUp> source)
    {
        List<TUp> values = RemoveDuplicated(source).ToList();
        return values.Select(MapRecord).ToList();
    }

    /// <summary>
    /// Removes duplicated info (PK), persisting last record on list
    /// </summary>
    /// <param name="source">List of items to process</param>
    /// <returns></returns>
    private IEnumerable<TUp> RemoveDuplicated(List<TUp> source)
    {
        // Removes duplicated entries
        Dictionary<string, TUp> dictionary = new();
        source.ForEach(x => dictionary[GroupBy(x)] = x);
        return dictionary.Values;
    }
}