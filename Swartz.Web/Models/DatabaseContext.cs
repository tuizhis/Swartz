using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using MySql.Data.Entity;
using Swartz.Users.Models;

namespace Swartz.Web.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class DatabaseContext : SwartzDbContext<User>
    {
        public DatabaseContext() : this("DefaultConnection")
        {
        }

        public DatabaseContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public DatabaseContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public DatabaseContext(DbCompiledModel model) : base(model)
        {
        }

        public DatabaseContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public DatabaseContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}