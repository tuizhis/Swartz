using Autofac;
using Swartz.Environment.Configuration;
using Swartz.Logging;

namespace Swartz.Environment.ShellBuilders
{
    public interface IShellContextFactory
    {
        /// <summary>
        ///     Builds a shell context given a specific tenant settings structure
        /// </summary>
        ShellContext CreateShellContext(ShellSettings settings);
    }

    public class ShellContextFactory : IShellContextFactory
    {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;

        public ShellContextFactory(ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory)
        {
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ShellContext CreateShellContext(ShellSettings settings)
        {
            var blueprint = _compositionStrategy.Compose(settings);
            var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext
            {
                Blueprint = blueprint,
                LifetimeScope = shellScope,
                Settings = settings,
                Shell = shellScope.Resolve<IWebShell>()
            };
        }
    }
}