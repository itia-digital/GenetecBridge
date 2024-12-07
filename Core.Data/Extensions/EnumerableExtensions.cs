namespace Core.Data.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Compact<T>(
        this IEnumerable<T?> enumerable) where T : class
        => enumerable.Where(i => i != null)!;
    
    public static IEnumerable<T> Compact<T>(
        this IEnumerable<T?> enumerable) where T : struct
        => enumerable.Where(i => i != null).Select(i => i!.Value);
}