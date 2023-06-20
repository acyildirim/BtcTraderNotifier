using System.Linq.Expressions;

namespace BtcTrader.Data.Repositories.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
       
        IEnumerable<TEntity> FilterBy(Expression<Func<TEntity, bool>> filterExpression);
        IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");
        IQueryable<TEntity> AsQueryable();

        TEntity FindOne(Expression<Func<TEntity, bool>> filterExpression);

        Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> filterExpression);

        IQueryable<TEntity> GetFromRawSql(string query);
        TEntity GetByID(object id);
        Task<TEntity> GetByIDAsync(object id);
        bool Insert(TEntity entity);
        Task<bool> InsertAsync(TEntity entity);
        bool Update(TEntity entity);
        bool Remove(TEntity entity);

    }
}