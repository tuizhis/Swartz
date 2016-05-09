﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Swartz.Data
{
    public class Repository<TKey, TEntity> : IRepository<TKey, TEntity> where TKey : IEquatable<TKey>
        where TEntity : class
    {
        private readonly ITransactionManager _transactionManager;

        public Repository(ITransactionManager transactionManager)
        {
            _transactionManager = transactionManager;
        }

        protected virtual DbContext DbContext => _transactionManager.GetContext();

        private DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

        public virtual IQueryable<TEntity> Table => DbContext.Set<TEntity>();

        public virtual TEntity Get(TKey id)
        {
            return DbSet.Find(id);
        }

        public virtual TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.SingleOrDefault(predicate);
        }

        public virtual void Create(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public virtual void CreateRange(IEnumerable<TEntity> entities)
        {
            DbSet.AddRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.Count(predicate);
        }

        public virtual IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> predicate)
        {
            return Table.Where(predicate);
        }

        public virtual IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> predicate,
            Action<Orderable<TEntity>> order)
        {
            var orderable = new Orderable<TEntity>(Fetch(predicate));
            order(orderable);
            return orderable.Queryable;
        }

        public virtual IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> predicate,
            Action<Orderable<TEntity>> order, int skip, int count)
        {
            return Fetch(predicate, order).Skip(skip).Take(count);
        }

        #region IRepository<T> Members

        IQueryable<TEntity> IRepository<TKey, TEntity>.Table => Table;

        int IRepository<TKey, TEntity>.Count(Expression<Func<TEntity, bool>> predicate)
        {
            return Count(predicate);
        }

        void IRepository<TKey, TEntity>.Create(TEntity entity)
        {
            Create(entity);
        }

        void IRepository<TKey, TEntity>.Delete(TEntity entity)
        {
            Delete(entity);
        }

        IQueryable<TEntity> IRepository<TKey, TEntity>.Fetch(Expression<Func<TEntity, bool>> predicate)
        {
            return Fetch(predicate);
        }

        IQueryable<TEntity> IRepository<TKey, TEntity>.Fetch(Expression<Func<TEntity, bool>> predicate,
            Action<Orderable<TEntity>> order)
        {
            return Fetch(predicate, order);
        }

        IQueryable<TEntity> IRepository<TKey, TEntity>.Fetch(Expression<Func<TEntity, bool>> predicate,
            Action<Orderable<TEntity>> order, int skip, int count)
        {
            return Fetch(predicate, order, skip, count);
        }

        TEntity IRepository<TKey, TEntity>.Get(Expression<Func<TEntity, bool>> predicate)
        {
            return Get(predicate);
        }

        TEntity IRepository<TKey, TEntity>.Get(TKey id)
        {
            return Get(id);
        }

        void IRepository<TKey, TEntity>.Update(TEntity entity)
        {
            Update(entity);
        }

        void IRepository<TKey, TEntity>.CreateRange(IEnumerable<TEntity> entities)
        {
            CreateRange(entities);
        }

        #endregion
    }

    public class Repository<TEntity> : Repository<decimal, TEntity>, IRepository<TEntity> where TEntity : class
    {
        public Repository(ITransactionManager transactionManager) : base(transactionManager)
        {
        }
    }
}