using System.Linq.Expressions;
using BtcTrader.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BtcTrader.Data.Repositories.Implementations
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        public Repository(ApplicationDbContext context)
        {
            this._context = context;
            this._dbSet = context.Set<TEntity>();
        }

        public IEnumerable<TEntity> FilterBy(Expression<Func<TEntity, bool>> filterExpression)
        {
            return _dbSet.Where(filterExpression);
        }

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            return orderBy != null ? orderBy(query) : query;
        }
        public IEnumerable<TEntity> AsEnumerable()
        {
            return _dbSet.AsEnumerable();
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<List<TEntity>> AsQueryableAsync()
        {
            return await _dbSet.AsQueryable().ToListAsync();
        }

        public virtual TEntity FindOne(Expression<Func<TEntity, bool>> filterExpression)
        {
            return _dbSet.FirstOrDefault(filterExpression);
        }

        public virtual Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return _dbSet.FirstOrDefaultAsync(filterExpression);
        }

        public IQueryable<TEntity> GetFromRawSql(string query)
        {
            var queryableEntity = _dbSet.FromSqlRaw(query);
            return queryableEntity;
        }

        public virtual TEntity GetByID(object id)
        {
            return _dbSet.Find(id);
        }
        public virtual async Task<TEntity> GetByIDAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }
        public virtual bool Insert(TEntity entity)
        {
            _dbSet.Add(entity);
            var isSuccess = _context.SaveChanges() > 0;
            return isSuccess;
        }
        public virtual async Task<bool> InsertAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            var isSuccess = await _context.SaveChangesAsync() > 0;
            return isSuccess;
        }

        public bool Update(TEntity entity)
        {
            _dbSet.Update(entity);
            var isSuccess = _context.SaveChanges() > 0;
            return isSuccess;
        }

        public bool Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
            var isSuccess = _context.SaveChanges() > 0;
            return isSuccess;
        }

    }
}