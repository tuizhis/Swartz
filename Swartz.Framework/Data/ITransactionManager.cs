using System.Data.Entity;

namespace Swartz.Data
{
    public interface ITransactionManager : IDependency
    {
        DbContext GetContext();

        void Save();
    }
}