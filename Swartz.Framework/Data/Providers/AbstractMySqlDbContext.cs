using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using MySql.Data.Entity;

namespace Swartz.Data.Providers
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public abstract class AbstractMySqlDbContext : DbContext
    {
        protected AbstractMySqlDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        protected AbstractMySqlDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        protected AbstractMySqlDbContext(DbConnection connection) : this(connection, true)
        {
        }

        protected AbstractMySqlDbContext(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
        }

        protected AbstractMySqlDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        protected AbstractMySqlDbContext(DbConnection existingConnection, DbCompiledModel model,
            bool contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}