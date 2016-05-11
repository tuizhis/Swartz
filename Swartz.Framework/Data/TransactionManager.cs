using System;
using System.Data.Entity;

namespace Swartz.Data
{
    public abstract class TransactionManager : ITransactionManager, IDisposable
    {
        private DbContext _dbContext;

        public void Dispose()
        {
            DisposeDbContext();
        }

        public DbContext GetContext()
        {
            EnsureDbContext();
            return _dbContext;
        }

        public void Save()
        {
            _dbContext?.SaveChanges();

            DisposeDbContext();
        }

        private void EnsureDbContext()
        {
            if (_dbContext != null)
            {
                return;
            }

            _dbContext = RequireNewDbContext();
        }

        protected virtual void DisposeDbContext()
        {
            _dbContext?.Dispose();
            _dbContext = null;
        }

        protected abstract DbContext RequireNewDbContext();
    }
}