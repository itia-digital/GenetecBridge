namespace Core.Data.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    ///     Removes duplicated info (PK), persisting last record on list
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
    
    /// <summary>
    /// When we call this method on an array that contains null elements, it removes them.
    /// It only returns the other, non-null, elements of the array.
    /// </summary>
    /// <param name="enumerable">IEnumerable data source object</param>
    /// <typeparam name="T">Base object type</typeparam>
    /// <returns>IEnumerable that contains not-null elements</returns>
    public static IEnumerable<T> Compact<T>(
        this IEnumerable<T?> enumerable
    ) where T : class
        => enumerable.Where(_ => _ != null)!;
}