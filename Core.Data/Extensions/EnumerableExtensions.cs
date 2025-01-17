namespace Core.Data.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Removes duplicated info (PK), persisting last record on list
    /// </summary>
    /// <param name="source">List of items to process</param>
    /// <param name="getPkFunc">Entity's Primary Key</param>
    /// <typeparam name="TEntity">DB entity</typeparam>
    /// <returns></returns>
    public static IEnumerable<TEntity> RemoveDuplicated<TEntity>(
        this List<TEntity> source, Func<TEntity, string> getPkFunc)
    {
        Dictionary<string, TEntity> values = new();
        source.ForEach(x => values[getPkFunc(x)] = x);
        return values.Select(x => x.Value);
    }
}