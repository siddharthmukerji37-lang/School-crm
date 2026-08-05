namespace SchoolCRM.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortColumn, string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortColumn)) return query;

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, sortColumn);
        var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

        var methodName = sortOrder?.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);

        return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }

    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? searchTerm, params System.Linq.Expressions.Expression<Func<T, string>>[] properties)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || properties.Length == 0) return query;

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        System.Linq.Expressions.Expression? combinedExpression = null;

        foreach (var property in properties)
        {
            var body = System.Linq.Expressions.Expression.Invoke(property, parameter);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            var searchConstant = System.Linq.Expressions.Expression.Constant(searchTerm.ToLower());
            var containsExpression = System.Linq.Expressions.Expression.Call(body, containsMethod, searchConstant);

            combinedExpression = combinedExpression == null
                ? containsExpression
                : System.Linq.Expressions.Expression.OrElse(combinedExpression, containsExpression);
        }

        if (combinedExpression == null) return query;

        var lambdaExpression = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return query.Where(lambdaExpression);
    }
}
