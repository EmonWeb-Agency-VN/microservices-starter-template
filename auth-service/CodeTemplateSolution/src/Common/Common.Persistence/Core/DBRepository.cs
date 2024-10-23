using System.Linq.Expressions;
using Common.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Common.Persistence.Core
{
    public class DBRepository : IDBRepository
    {

        public DbContext Context { get; }

        public DBRepository()
        {

        }

        public DBRepository(UserDbContext context)
        {
            context.Database.SetCommandTimeout(180);
            Context = context;
        }

        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            var entry = await Context.Set<T>().AddAsync(entity);
            return entry.Entity;
        }

        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            await Context.Set<T>().AddRangeAsync(entities);
        }

        public int Delete<T>(T entity) where T : class
        {
            Context.Set<T>().Remove(entity);
            return 1;
        }

        public int Delete<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var removeList = Context.Set<T>().Where(predicate);
            Context.Set<T>().RemoveRange(removeList);
            return removeList.Count();
        }

        public async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await Task.Run(() => Delete(predicate), default);
        }

        public int DeleteRange<T>(IEnumerable<T> entities) where T : class
        {
            Context.Set<T>().RemoveRange(entities);
            return entities.Count();
        }

        public void Dispose()
        {
        }

        public async Task<int> ExecuteSqlCommandAsync(string sql, params object[] param)
        {
            return await Context.Database.ExecuteSqlRawAsync(sql, param);
        }

        public async Task<int> ExecuteSqlCommandAsync(string formattedSql)
        {
            return await Context.Database.ExecuteSqlRawAsync(formattedSql);
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TResult, bool>> expression, bool track = true) where TResult : class
        {
            return track
               ? await Context.Set<TResult>().FirstOrDefaultAsync(expression)
               : await Context.Set<TResult>().Where(expression).AsNoTracking().FirstOrDefaultAsync();
        }

        public IQueryable<T> FromSql<T>(string sql, params object[] param) where T : class
        {
            return Context.Set<T>().FromSqlRaw(sql, param);
        }

        public IQueryable<T> FromSql<T>(string formattedSql) where T : class
        {
            return Context.Set<T>().FromSqlRaw(formattedSql);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }

        public int Update<T>(T entity) where T : class
        {
            Context.Set<T>().Update(entity);
            return 1;
        }

        public async Task<int> UpdateAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateFactory) where T : class
        {
            var updateList = await Context.Set<T>().Where(predicate).ToListAsync();
            var memberInitExpression = updateFactory.Body as MemberInitExpression;
            ArgumentNullException.ThrowIfNull(memberInitExpression);
            foreach (MemberBinding binding in memberInitExpression.Bindings)
            {
                string propertyName = binding.Member.Name;
                var memberAssignment = binding as MemberAssignment;
                ArgumentNullException.ThrowIfNull(memberAssignment);
                object value;
                if (memberAssignment.Expression.NodeType == ExpressionType.Constant)
                {
                    var constantExpression = memberAssignment.Expression as ConstantExpression;
                    ArgumentNullException.ThrowIfNull(constantExpression);
                    value = constantExpression.Value;
                }
                else
                {
                    LambdaExpression lambda = Expression.Lambda(memberAssignment.Expression, null);
                    value = lambda.Compile().DynamicInvoke();
                }
                updateList.ForEach(t => t.GetType().GetProperty(propertyName).SetValue(t, value));
            }
            return updateList.Count;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await Context.Database.BeginTransactionAsync();
        }

        public async Task<int> ExecuteUpdateAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression, CancellationToken cancellationToken = default) where T : class
        {
            return await Context.Set<T>().Where(predicate).ExecuteUpdateAsync(updateExpression, cancellationToken);
        }

    }
}
