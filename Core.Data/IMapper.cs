namespace Core.Data;

public interface IMapper<in TSource, out T> 
    where TSource : class
    where T : class
{
    T Map(TSource source);
}