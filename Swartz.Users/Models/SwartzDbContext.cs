using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Swartz.Users.Models
{
    public class SwartzDbContext<TUser, TRole, TKey, TUserRole> : DbContext where TUser : SwartzUser<TKey>
        where TRole : SwartzRole<TKey>
        where TUserRole : SwartzUserRole<TKey>
    {
        public SwartzDbContext() : this("DefaultConnection")
        {
        }

        public SwartzDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public SwartzDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public SwartzDbContext(DbCompiledModel model) : base(model)
        {
        }

        public SwartzDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public SwartzDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public virtual IDbSet<TUser> Users { get; set; }

        public virtual IDbSet<TRole> Roles { get; set; }

        public virtual IDbSet<TUserRole> UserRoles { get; set; }
    }
}