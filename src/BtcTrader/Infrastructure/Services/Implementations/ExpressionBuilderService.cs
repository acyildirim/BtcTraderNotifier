using System.Linq.Expressions;
using BtcTrader.Infrastructure.Services.Abstractions;

namespace BtcTrader.Infrastructure.Services.Implementations;

public abstract class ExpressionBuilderService<TEntity> : IExpressionBuilderService<TEntity> where TEntity : class
{
  
    public Expression<Func<TEntity, bool>> GetExpression(Dictionary<string,string>?  filterBy)
    {
        var expression = this.GetQueryExpression<TEntity>(filterBy);

        return expression;
    }
    
    protected abstract Expression<Func<TDocument, bool>> GetQueryExpression<TDocument>(Dictionary<string,string>?  filterBy);

}