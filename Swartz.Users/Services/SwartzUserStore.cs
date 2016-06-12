using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Swartz.Users.Models;

namespace Swartz.Users.Services
{
    public class SwartzUserStore<TUser, TRole, TUserRole, TKey> : IUserPasswordStore<TUser, TKey>,
        IUserEmailStore<TUser, TKey>, IQueryableUserStore<TUser, TKey>, ISwartzUserPhoneNumberStore<TUser, TKey>,
        IUserSecurityStampStore<TUser, TKey>, IUserLockoutStore<TUser, TKey>
        where TUser : SwartzUser<TKey>
        where TRole : SwartzRole<TKey>
        where TUserRole : SwartzUserRole<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        private readonly DbSet<TRole> _roleStore;
        private readonly DbSet<TUserRole> _userRoles;
        private bool _disposed;
        private DbSet<TUser> _userStore;

        public SwartzUserStore(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Context = context;
            _userStore = Context.Set<TUser>();
            _roleStore = Context.Set<TRole>();
            _userRoles = Context.Set<TUserRole>();
            AutoSaveChanges = true;
        }

        public DbContext Context { get; private set; }

        public bool DisposeContext { get; set; }

        public bool AutoSaveChanges { get; set; }

        public IQueryable<TUser> Users => _userStore;

        public async Task<TUser> FindByPhoneNumberAsync(string phone)
        {
            ThrowIfDisposed();
            return await Users.SingleOrDefaultAsync(x => x.Phone == phone);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Phone = phoneNumber;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetPhoneNumberAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.Phone);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public virtual Task SetEmailAsync(TUser user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.Email = email;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetEmailAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Email);
        }

        public virtual Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.EmailConfirmed);
        }

        public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public virtual async Task<TUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            return await Users.SingleOrDefaultAsync(x => x.Email.ToUpper() == email.ToUpper());
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue
                ? new DateTime?()
                : lockoutEnd.UtcDateTime;

            return Task.FromResult(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            ++user.AccessFailedCount;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual async Task CreateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _userStore.Add(user);
            await SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Context.Entry(user).State = EntityState.Modified;
            await SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _userStore.Remove(user);
            await SaveChangesAsync();
        }

        public virtual async Task<TUser> FindByIdAsync(TKey userId)
        {
            ThrowIfDisposed();
            return await _userStore.FindAsync(userId);
        }

        public virtual async Task<TUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            return await Users.SingleOrDefaultAsync(x => x.UserName.ToUpper() == userName.ToUpper());
        }

        public virtual Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Password = passwordHash;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetPasswordHashAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.Password);
        }

        public virtual Task<bool> HasPasswordAsync(TUser user)
        {
            return Task.FromResult(user.Password != null);
        }

        public virtual Task SetSecurityStampAsync(TUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetSecurityStampAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.SecurityStamp);
        }

        public virtual async Task AddToRoleAsync(TUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(roleName));
            }

            var role = await _roleStore.SingleOrDefaultAsync(x => x.Name.ToUpper() == roleName.ToUpper());
            if (role == null)
            {
                throw new InvalidOperationException($"Role {roleName} does not exist.");
            }

            var userRole = new TUserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            _userRoles.Add(userRole);
        }

        public virtual async Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(roleName));
            }

            var role = await _roleStore.SingleOrDefaultAsync(x => x.Name.ToUpper() == roleName.ToUpper());
            if (role != null)
            {
                var entity =
                    await _userRoles.FirstOrDefaultAsync(r => r.RoleId.Equals(role.Id) && r.UserId.Equals(user.Id));
                if (entity != null)
                {
                    _userRoles.Remove(entity);
                }
            }
        }

        public virtual async Task<IList<string>> GetRolesAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userId = user.Id;
            var source = from userRole in _userRoles
                where userRole.UserId.Equals(userId)
                select userRole
                into userRole
                join role in _roleStore on userRole.RoleId equals role.Id
                select role.Name;

            return await source.ToListAsync();
        }

        public virtual async Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(roleName));
            }

            var role = await _roleStore.SingleOrDefaultAsync(x => x.Name.ToUpper() == roleName.ToUpper());
            if (role != null)
            {
                return await _userRoles.AnyAsync(ur => ur.RoleId.Equals(role.Id) && ur.UserId.Equals(user.Id));
            }

            return false;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private async Task SaveChangesAsync()
        {
            if (!AutoSaveChanges)
            {
                return;
            }
            await Context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (DisposeContext && disposing)
            {
                Context?.Dispose();
            }
            _disposed = true;
            Context = null;
            _userStore = null;
        }
    }

    public class SwartzUserStore<TUser> : SwartzUserStore<TUser, SwartzRole, SwartzUserRole, decimal>
        where TUser : SwartzUser<decimal>
    {
        public SwartzUserStore(DbContext context) : base(context)
        {
        }
    }
}