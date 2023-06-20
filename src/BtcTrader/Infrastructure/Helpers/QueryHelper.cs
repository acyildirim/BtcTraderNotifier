using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using BtcTrader.Infrastructure.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace BtcTrader.Infrastructure.Helpers;

public static class QueryHelper
{
    private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
    private static readonly MethodInfo ToStringMethod = typeof(string).GetMethod("ToString", System.Type.EmptyTypes);

    private static readonly MethodInfo ContainsMethod =
        typeof(string).GetMethod("Contains", new[] { typeof(string) });
    
    private static readonly MethodInfo StartsWithMethod =
        typeof(string).GetMethod("StartsWith", new[] { typeof(string) });

    private static readonly MethodInfo EndsWithMethod =
        typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

    public static Expression<Func<T, bool>> GetExpression<T>(IList<QueryFilter> filters)
    {
        Expression exp = null;

        var param = Expression.Parameter(typeof(T), "parm");
        foreach (var filter in filters)
        {
            if (filter.Value is string && filter.Value.Contains(";"))
            {
                filter.Operator = Operator.InList;
            }
        }

            
        switch (filters.Count)
        {
            case 0:
                return null;
            case 1:
                exp = GetExpression<T>(param, filters[0]);
                break;
            case 2:
                exp = GetExpression<T>(param, filters[0], filters[1]);
                break;
            default:
            {
                while (filters.Count > 0)
                {
                    var f1 = filters[0];
                    var f2 = filters[1];

                    exp = exp == null
                        ? GetExpression<T>(param, filters[0], filters[1])
                        : Expression.AndAlso(exp, GetExpression<T>(param, filters[0], filters[1]));

                    filters.Remove(f1);
                    filters.Remove(f2);

                    if (filters.Count == 1)
                    {
                        exp = Expression.AndAlso(exp, GetExpression<T>(param, filters[0]));
                        filters.RemoveAt(0);
                    }
                }

                break;
            }
        }

        return Expression.Lambda<Func<T, bool>>(exp, param);
    }

    private static Expression GetExpression<T>(ParameterExpression param, QueryFilter queryFilter)
    {
        // Represents accessing a field or property, so here we are accessing PropertyName:
        var member = Expression.Property(param, queryFilter.PropertyName);
        
        var constant = GetConstant(member.Type, queryFilter.Value);
        
        switch (queryFilter.Operator)
        {
            case Operator.Equals:
                if (member.Type != typeof(Guid)) return Expression.Equal(member, constant);
                var toString = Expression.Call(member, "ToString", Type.EmptyTypes);
                return Expression.Equal(toString, constant);
            case Operator.Contains:
                var toLower = Expression.Call(member, ToLowerMethod);
                return Expression.Call(toLower,
                    ContainsMethod,
                    Expression.Constant(queryFilter.Value.ToLower()));
            case Operator.GreaterThan:
                return Expression.GreaterThan(member, constant);
            case Operator.GreaterThanOrEqual:
                return Expression.GreaterThanOrEqual(member, constant);
            case Operator.LessThan:
                return Expression.LessThan(member, constant);
            case Operator.LessThanOrEqualTo:
                return Expression.LessThanOrEqual(member, constant);
            case Operator.StartsWith:
                return Expression.Call(member, StartsWithMethod, constant);
            case Operator.InList:
                var queryValues = constant.Value?.ToString()?.Split(";").ToList()!;
                Expression exp = null;
                foreach (var value in queryValues)
                {
                    var constantValue = Expression.Constant(value);
                    Expression secondExp = Expression.Call(member, ContainsMethod, constantValue);
                    exp = exp == null
                        ?  Expression.Call(member, ContainsMethod, constantValue)
                        : Expression.Or(exp,secondExp );
                }
                return exp;
            case Operator.EndsWith:
                return Expression.Call(member, EndsWithMethod, constant);
            default:
                return null;
        }
    }
    private static BinaryExpression GetExpression<T>(ParameterExpression param, QueryFilter filter1,
        QueryFilter filter2)
    {
        Expression result1 = GetExpression<T>(param, filter1);
        Expression result2 = GetExpression<T>(param, filter2);
        return Expression.AndAlso(result1, result2);
    }

    private static ConstantExpression GetConstant(Type type, string value)
    {
        // Discover the type, convert it, and create ConstantExpression 
        ConstantExpression constant = null;
        if (type == typeof(int))
        {
            int num;
            int.TryParse(value, out num);
            constant = Expression.Constant(num);
        }
        else if (type == typeof(string))
        {
            constant = Expression.Constant(value);
        }
        else if (type == typeof(DateTime))
        {
            DateTime date;
            DateTime.TryParse(value, out date);
            constant = Expression.Constant(date);
        }
        else if (type == typeof(bool))
        {
            var flag = bool.Parse(value);

            constant = Expression.Constant(flag);
        }
        else if (type == typeof(decimal))
        {
            decimal number;
            decimal.TryParse(value, out number);
            constant = Expression.Constant(number);
        }
        else if (type== typeof(Guid))
        {
            constant = Expression.Constant(value);
        }
        else if (type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var queryValues =value.Split(";").ToList();
            constant = Expression.Constant(queryValues);
        }
       

        return constant;
    }

    public static Dictionary<string, string> GenerateFilterFromQueryString(string queryString)
    {
        var filter = new Dictionary<string, string>();
        if (queryString == null) return filter;
        var parsedQueryString = QueryHelpers.ParseQuery(queryString);
        foreach (var (key, value) in parsedQueryString)
        {
            if (key.ToLower() is "page" or "size") continue;
            filter.Add(key, value.ToString());
        }

        return filter;
    }

}