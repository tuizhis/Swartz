using System.Collections.Generic;

namespace Swartz.Owin
{
    /// <summary>
    ///     Represents a provider that makes Owin middlewares available for the Owin pipeline.
    /// </summary>
    /// <remarks>
    ///     If you want to write an Owin middleware and inject it into the Owin pipeline, implement this interface. For
    ///     more information
    ///     about Owin <see cref="!:http://owin.org/">http://owin.org/</see>.
    /// </remarks>
    public interface IOwinMiddlewareProvider : IDependency
    {
        /// <summary>
        ///     Gets <see cref="OwinMiddlewareRegistration" /> objects that will be used to alter the Owin pipeline.
        /// </summary>
        /// <returns><see cref="OwinMiddlewareRegistration" /> objects that will be used to alter the Owin pipeline.</returns>
        IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares();
    }
}