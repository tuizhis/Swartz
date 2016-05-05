namespace Swartz.Environment
{
    /// <summary>
    ///     Describes a service which returns the a machine identifier running the application.
    /// </summary>
    public interface IApplicationEnvironment
    {
        /// <summary>
        ///     Returns the machine identifier running the application.
        /// </summary>
        /// <returns></returns>
        string GetEnvironmentIdentifier();
    }
}