using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Swartz.Data
{
    public interface IRepository<in TKey, TEntity> where TKey : IEquatable<TKey> where TEntity : class
    {
        IQueryable<TEntity> Table { get; }

        void Create(TEntity entity);
        void CreateRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        TEntity Get(TKey id);

        TEntity Get(Expression<Func<TEntity, bool>> predicate);

        int Count(Expression<Func<TEntity, bool>> predicate);

        IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> predicate);
        IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> predicate, Action<Orderable<TEntity>> order);

        IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> predicate, Action<Orderable<TEntity>> order, int skip,
            int count);
    }

    public interface IRepository<TEntity> : IRepository<decimal, TEntity> where TEntity : class
    {
    }
}