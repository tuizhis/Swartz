using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Swartz.Users.Models;

namespace Swartz.Users.Services
{
    public class SwartzRoleStore<TRole, TKey> : IQueryableRoleStore<TRole, TKey> where TRole : SwartzRole<TKey>, new()
    {
        private bool _disposed;
        private DbSet<TRole> _roleStore;

        public SwartzRoleStore(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Context = context;
            _roleStore = Context.Set<TRole>();
        }

        public DbContext Context { get; private set; }

        public bool DisposeContext { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual async Task CreateAsync(TRole role)
        {
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _roleStore.Add(role);
            await Context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TRole role)
        {
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            Context.Entry(role).State = EntityState.Modified;
            await Context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TRole role)
        {
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _roleStore.Remove(role);
            await Context.SaveChangesAsync();
        }

        public virtual async Task<TRole> FindByIdAsync(TKey roleId)
        {
            ThrowIfDisposed();
            return await _roleStore.FindAsync(roleId);
        }

        public virtual async Task<TRole> FindByNameAsync(string roleName)
        {
            ThrowIfDisposed();
            return await _roleStore.SingleOrDefaultAsync(x => x.Name.ToUpper() == roleName.ToUpper());
        }

        public IQueryable<TRole> Roles => _roleStore;

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (DisposeContext && disposing)
            {
                Context?.Dispose();
            }

            _disposed = true;
            Context = null;
            _roleStore = null;
        }
    }

    public class SwartzRoleStore<TRole> : SwartzRoleStore<TRole, decimal> where TRole : SwartzRole, new()
    {
        public SwartzRoleStore(DbContext context) : base(context)
        {
        }
    }
}