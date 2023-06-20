using System.Linq.Expressions;
using BtcTrader.Infrastructure.Services.Abstractions;

namespace BtcTrader.Infrastructure.Services.Implementations;

public class QueryService<TEntity> : IQueryService<TEntity> where TEntity : class
{
    private readonly IExpressionBuilderService<TEntity> _expressionBuilderService;

    public QueryService(IExpressionBuilderService<TEntity> expressionBuilderService)
    {
        _expressionBuilderService = expressionBuilderService;
    }

    public Expression<Func<TEntity, bool>> GetExpression(Dictionary<string,string>? filterBy)
    {
        return _expressionBuilderService.GetExpression(filterBy);
    }
}