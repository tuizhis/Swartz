using Autofac;

namespace Swartz.Data
{
    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(Repository<,,>)).As(typeof(IRepository<,,>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(Repository<,>)).As(typeof(IRepository<,>)).InstancePerDependency();
        }
    }
}