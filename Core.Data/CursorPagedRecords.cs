using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Core.Data.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
namespace Core.Data;


public enum CursorPaginationDirection
{
    Backward,
    Forward
}

public record CursorQueryParams(
    [FromQuery] string? After,
    [FromQuery] string? Before,
    int PageSize = 10)
{
    public CursorPaginationDirection Direction
        => !string.IsNullOrEmpty(Before)
            ? CursorPaginationDirection.Backward
            : CursorPaginationDirection.Forward;

    public string? Cursor
        => string.IsNullOrEmpty(After)
            ? Before
            : After;
}

public record SortingCursorSetup(string PropName, bool ByDescending, bool IsRecordId, string? coalesce = null)
{
    public static SortingCursorSetup Default(string propName)
        => new(propName, false, false);

    public static SortingCursorSetup DefaultDescending(string propName)
        => new(propName, true, false);

    public static SortingCursorSetup DefaultRecordId(string propName)
        => new(propName, false, true);
}

public record CursorPagedRecords<T>(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    IEnumerable<T> Items,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string? Before = null,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string? After = null
)
{
    public static CursorPagedRecords<T> Default => new(ArraySegment<T>.Empty);
}

/// <summary>
/// The combination of Direction and Cursors is
/// equivalent to say [after/before]: [cursor]
/// </summary>
/// <param name="Sorting"></param>
/// <param name="Direction"></param>
/// <param name="Cursor"></param>
/// <param name="PageSize"></param>
public record PageQueryCursorSetup(
    IEnumerable<SortingCursorSetup> Sorting,
    CursorPaginationDirection Direction,
    Dictionary<string, object> Cursor,
    int PageSize
)
{
    private SortingCursorSetup? _sortingId;

    public SortingCursorSetup? SortingId
        => _sortingId ??= Sorting.FirstOrDefault(e => e.IsRecordId);

    public object? IdValue => SortingId != null
        ? Cursor.GetValueOrDefault(SortingId.PropName)
        : null;
}

public static partial class CursorPagination
{
    #region Q u e r y

    public static async Task<List<TEntity>> ToListAsync<TEntity>(
        this IQueryable<TEntity> query,
        CursorQueryParams queryParams,
        params SortingCursorSetup[] sorting)
        where TEntity : class
    {
        var setup = new PageQueryCursorSetupMapper(sorting).ToDto(queryParams);
        IQueryable<TEntity> q = query.ToCursorQuery(setup);

        List<TEntity> result = await q.ToListAsync();

        if (queryParams.Direction == CursorPaginationDirection.Backward)
        {
            result.Reverse();
        }

        return result;
    }

    private static IQueryable<TEntity> ToCursorQuery<TEntity>(
        this IQueryable<TEntity> query,
        PageQueryCursorSetup setup)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!setup.Sorting.Any())
        {
            throw new ArgumentException(
                "Sorting is required by the cursor pagination",
                nameof(setup.Sorting));
        }

        // sort by direction
        query = setup.Sorting
            .Aggregate(query,
                (current, sort)
                    => ApplySorting(current, sort, setup.Direction)
            );

        // filter by keySet
        query = setup.Cursor
            .Aggregate(query,
                (current, key)
                    => ApplyKeySetFilter(current, setup, key.Key, key.Value)
            );

        return query.Take(setup.PageSize);
    }

    private static IOrderedQueryable<T> ApplySorting<T>(IQueryable<T> query,
        SortingCursorSetup sort, CursorPaginationDirection direction)
        where T : class
    {
        bool isNotOrdered = OrderedQueryRegex().Match(query.Expression.Type.Name).Success;

        Expression<Func<T, object>> sorting = SortingExpression<T>(sort.PropName, sort.coalesce);

        if (isNotOrdered)
        {
            return direction switch
            {
                CursorPaginationDirection.Backward => sort.ByDescending
                    ? query.OrderBy(sorting)
                    : query.OrderByDescending(sorting),
                CursorPaginationDirection.Forward => sort.ByDescending
                    ? query.OrderByDescending(sorting)
                    : query.OrderBy(sorting),
                _ => throw new ArgumentOutOfRangeException(nameof(direction),
                    direction,
                    null)
            };
        }

        IOrderedQueryable<T> oQuery = (IOrderedQueryable<T>)query;
        return direction switch
        {
            CursorPaginationDirection.Backward => sort.ByDescending
                ? oQuery.ThenBy(sorting)
                : oQuery.ThenByDescending(sorting),
            CursorPaginationDirection.Forward => sort.ByDescending
                ? oQuery.ThenByDescending(sorting)
                : oQuery.ThenBy(sorting),
            _ => throw new ArgumentOutOfRangeException(nameof(direction),
                direction,
                null)
        };
    }

    private static IQueryable<T> ApplyKeySetFilter<T>(IQueryable<T> query,
        PageQueryCursorSetup setup, string key, object value)
        where T : class
    {
        SortingCursorSetup sort = setup.Sorting.First(e => e.PropName == key);

        // applies filter by id only when is the only present on setup
        if (setup.Sorting.Count() > 1 && sort.IsRecordId) { return query; }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

        if (sort.IsRecordId)
        {
            if (setup.SortingId == null || setup.IdValue == null) { return query; }

            Expression<Func<T, bool>> onlyByIdPredicate =
                Expression.Lambda<Func<T, bool>>(
                    ComparisonExpression(setup.SortingId.PropName, setup.IdValue),
                    parameter);
            return query.Where(onlyByIdPredicate);
        }

        Expression item = ComparisonExpression(sort.PropName, value);

        if (setup.SortingId == null || setup.IdValue == null)
        {
            Expression<Func<T, bool>> simplePredicate =
                Expression.Lambda<Func<T, bool>>(item, parameter);

            return query.Where(simplePredicate);
        }

        Expression itemOrEquals = ComparisonExpression(sort.PropName, value, true);
        Expression id = ComparisonExpression(setup.SortingId.PropName, setup.IdValue);
        Expression itemAndId = Expression.And(itemOrEquals, id);
        BinaryExpression combined = Expression.Or(item, itemAndId);

        Expression<Func<T, bool>> predicate =
            Expression.Lambda<Func<T, bool>>(combined, parameter);

        return query.Where(predicate);

        Expression ComparisonExpression(string popName, object v, bool orEquals = false)
        {
            MemberExpression member = Expression
                .PropertyOrField(parameter, popName);

            object? parsedValue = ParseValue(v.ToString()!, member.Type);
            ConstantExpression constant = Expression.Constant(parsedValue);
            bool isString = member.Type == typeof(string);

            return setup.Direction == CursorPaginationDirection.Forward
                ? sort.ByDescending
                    ? LessThan(member, constant, orEquals, isString)
                    : GreaterThan(member, constant, orEquals, isString)
                : sort.ByDescending
                    ? GreaterThan(member, constant, orEquals, isString)
                    : LessThan(member, constant, orEquals, isString);
        }

        object? ParseValue(string v, Type t)
        {
            if (t == typeof(DateTime)) { return DateTime.Parse(v); }

            if (t == typeof(DateTime?)) { return Parse<DateTime?>(v); }

            if (t == typeof(long)) { return long.Parse(v); }

            if (t == typeof(long?)) { return Parse<long?>(v); }

            if (t == typeof(int)) { return int.Parse(v); }

            if (t == typeof(int?)) { return Parse<int?>(v); }

            if (t == typeof(bool)) { return bool.Parse(v); }

            if (t == typeof(bool?)) { return Parse<bool?>(v); }

            return v;
        }

        static TProp? Parse<TProp>(string? v)
        {
            if (string.IsNullOrEmpty(v)) { return default; }

            try
            {
                return (TProp?)System.ComponentModel.TypeDescriptor
                    .GetConverter(typeof(TProp))
                    .ConvertFrom(v);
            }
            catch { return default; }
        }

        static Expression GreaterThan(Expression e1, Expression e2,
            bool orEquals, bool isString)
        {
            if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
                e2 = Expression.Convert(e2, e1.Type);
            else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
                e1 = Expression.Convert(e1, e2.Type);

            return isString
                ? GreaterThanByString(orEquals)
                : orEquals
                    ? Expression.GreaterThanOrEqual(e1, e2)
                    : Expression.GreaterThan(e1, e2);

            Expression GreaterThanByString(bool orEquals = false)
            {
                // Create the string.Compare call expression
                var compareMethod = typeof(string).GetMethod("Compare",
                    [typeof(string), typeof(string)])!;

                var compareCall = Expression.Call(compareMethod, e1, e2);

                return orEquals
                    ? Expression.GreaterThanOrEqual(compareCall, Expression.Constant(0))
                    : Expression.GreaterThan(compareCall, Expression.Constant(0));
            }
        }

        static Expression LessThan(Expression e1, Expression e2, bool orEquals,
            bool isString)
        {
            if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
                e2 = Expression.Convert(e2, e1.Type);
            else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
                e1 = Expression.Convert(e1, e2.Type);
            return isString
                ? LessThanByString(orEquals)
                : orEquals
                    ? Expression.LessThanOrEqual(e1, e2)
                    : Expression.LessThan(e1, e2);

            Expression LessThanByString(bool orEquals = false)
            {
                // Create the string.Compare call expression
                var compareMethod = typeof(string).GetMethod("Compare",
                    [typeof(string), typeof(string)])!;

                var compareCall = Expression.Call(compareMethod, e1, e2);

                return orEquals
                    ? Expression.LessThanOrEqual(compareCall, Expression.Constant(0))
                    : Expression.LessThan(compareCall, Expression.Constant(0));
            }
        }

        static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }

    private static Expression<Func<TEntity, object>> SortingExpression<TEntity>(string property, string? fallbackProperty)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression body;

        if (!string.IsNullOrEmpty(fallbackProperty))
        {
            // Access the primary property
            var primaryPropertyExpression = Expression.PropertyOrField(parameter, property);

            // Access the fallback property
            var fallbackPropertyExpression = Expression.PropertyOrField(parameter, fallbackProperty);

            // Coalesce expression (primary ?? fallback)
            body = Expression.Coalesce(
                Expression.Convert(primaryPropertyExpression, typeof(object)),
                Expression.Convert(fallbackPropertyExpression, typeof(object))
            );
        }
        else
        {
            body = Expression.Convert(
                Expression.PropertyOrField(parameter, property),
                typeof(object)
            );
        }
        return Expression.Lambda<Func<TEntity, object>>(body, parameter);
    }

    private class PageQueryCursorSetupMapper(SortingCursorSetup[] sorting)
    {
        public PageQueryCursorSetup ToDto(CursorQueryParams queryParams)
            => new(
                sorting,
                queryParams.Direction,
                GetCursorFromToken(queryParams.Cursor, sorting),
                queryParams.PageSize
            );

        private static Dictionary<string, object> GetCursorFromToken(string? token,
            SortingCursorSetup[] sortingSetup)
        {
            if (string.IsNullOrEmpty(token)) { return new Dictionary<string, object>(); }

            Dictionary<string, object> result
                = token.FromBase64().FromJson<Dictionary<string, object>>()
                  ?? throw new InvalidDataException("Invalid page token");

            return result
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                .Where(e => e.Value != null)
                .Join(sortingSetup,
                    r => r.Key,
                    s => s.PropName,
                    (r, _) => r)
                .ToDictionary();
        }
    }

    #endregion

    #region P a g i n a t i o n

    public static CursorPagedRecords<T> ToCursorPage<T>(
        this IEnumerable<T> items, SortingCursorSetup[] sortingSetup)
    {
        CursorTokenMapper<T> tokenMapper = new CursorTokenMapper<T>(sortingSetup);
        List<T> values = items.ToList();
        bool any = values.Count != 0;

        return new CursorPagedRecords<T>(values,
            any
                ? tokenMapper.ToDto(values.First())
                : null,
            any
                ? tokenMapper.ToDto(values.Last())
                : null
        );
    }

    private class CursorTokenMapper<T>(SortingCursorSetup[] sortingSetup)
    {
        public string ToDto(T item)
        {
            Dictionary<string, object> tokenValues = GetPaginationValues(item);
            return tokenValues.ToJson().ToBase64();
        }

        private Dictionary<string, object> GetPaginationValues(T source)
        {
            return sortingSetup
                .Select(kv =>
                {
                    object? value = GetPropertyValue(source, kv.PropName);
                    return value != null
                        ? new { kv.PropName, value }
                        : null;
                })
                .Compact()
                .ToDictionary(e => e.PropName, s => s.value);
        }

        private static object? GetPropertyValue(T source, string propertyName)
            => source
                ?.GetType()
                .GetProperty(propertyName)
                ?.GetValue(source, null);
    }

    [GeneratedRegex("IOrderedQueryable")]
    private static partial Regex OrderedQueryRegex();

    #endregion
}
