using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Common.Interfaces
{
    /// <summary>
    /// Generic Repository interface — cung cấp các thao tác CRUD cơ bản.
    /// Áp dụng cho mọi Aggregate Root trong Clean Architecture.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // ── READ ──────────────────────────────────────────────────────────
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Trả về IQueryable để Handlers có thể compose thêm filters/projections.
        /// </summary>
        IQueryable<T> Query();

        // ── WRITE ─────────────────────────────────────────────────────────
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
