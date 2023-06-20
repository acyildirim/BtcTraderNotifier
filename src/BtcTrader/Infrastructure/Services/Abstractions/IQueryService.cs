using System.Linq.Expressions;

namespace BtcTrader.Infrastructure.Services.Abstractions;

public interface IQueryService<TEntity> where TEntity : class
{
    Expression<Func<TEntity, bool>> GetExpression(Dictionary<string,string>?  filterBy);
}