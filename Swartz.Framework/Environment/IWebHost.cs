namespace Swartz.Environment
{
    public interface IWebHost
    {
        /// <summary>
        ///     Called once on startup to configure app domain, and load/apply existing shell configuration
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Called each time a request begins to offer a just-in-time reinitialization point
        /// </summary>
        void BeginRequest();

        /// <summary>
        ///     Called each time a request ends to deterministically commit and dispose outstanding activity
        /// </summary>
        void EndRequest();
    }
}