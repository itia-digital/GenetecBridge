namespace Core.Data.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Compact<T>(
        this IEnumerable<T?> enumerable) where T : class
        => enumerable.Where(i => i != null)!;
    
    public static IEnumerable<T> Compact<T>(
        this IEnumerable<T?> enumerable) where T : struct
        => enumerable.Where(i => i != null).Select(i => i!.Value);

    #region Data process

    /// <summary>
    /// Perform actions to prepare information to be inserted.
    /// </summary>
    /// <param name="source">List of items to process</param>
    /// <param name="getPkFunc">Entity's Primary Key</param>
    /// <param name="formatAction">Action to perform into each record before merge</param>
    /// <typeparam name="TEntity">DB entity</typeparam>
    /// <returns></returns>
    public static List<TEntity> Compile<TEntity>(List<TEntity> source, Func<TEntity, string> getPkFunc, Action<TEntity> formatAction)
    {
        List<TEntity> values = RemoveDuplicated(source, getPkFunc).ToList();
        values.ForEach(formatAction);
        return values;
    }

    /// <summary>
    /// Removes duplicated info (PK), persisting last record on list
    /// </summary>
    /// <param name="source">List of items to process</param>
    /// <param name="getPkFunc">Entity's Primary Key</param>
    /// <typeparam name="TEntity">DB entity</typeparam>
    /// <returns></returns>
    private static IEnumerable<TEntity> RemoveDuplicated<TEntity>(List<TEntity> source, Func<TEntity, string> getPkFunc)
    {
        // Removes duplicated entries
        Dictionary<string, TEntity> values = new();
        source.ForEach(x => values[getPkFunc(x)] = x);
        return values.Select(x => x.Value);
    }

    #endregion
    
}