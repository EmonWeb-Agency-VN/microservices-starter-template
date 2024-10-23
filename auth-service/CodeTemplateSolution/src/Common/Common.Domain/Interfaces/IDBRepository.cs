using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Common.Domain.Interfaces
{
    public interface IDBRepository : IDisposable
    {
        Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TResult, bool>> expression, bool track = true) where TResult : class;

        IQueryable<T> FromSql<T>(string sql, params object[] param) where T : class;

        IQueryable<T> FromSql<T>(string formattedSql) where T : class;

        Task<int> ExecuteSqlCommandAsync(string sql, params object[] param);

        Task<int> ExecuteSqlCommandAsync(string formattedSql);

        Task<T> AddAsync<T>(T entity) where T : class;

        Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class;

        int Update<T>(T entity) where T : class;

        Task<int> UpdateAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateFactory) where T : class;

        int Delete<T>(T entity) where T : class;

        int Delete<T>(Expression<Func<T, bool>> predicate) where T : class;

        int DeleteRange<T>(IEnumerable<T> entities) where T : class;

        Task<int> DeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));
        DbContext Context { get; }

        Task<int> ExecuteUpdateAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression, CancellationToken cancellationToken = default) where T : class;
    }
}
