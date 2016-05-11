using System;
using System.Web;

namespace Swartz
{
    public abstract class WorkContext
    {
        /// <summary>
        ///     The http context corresponding to the work context
        /// </summary>
        public HttpContextBase HttpContext
        {
            get { return GetState<HttpContextBase>("HttpContext"); }
            set { SetState("HttpContext", value); }
        }

        /// <summary>
        ///     Resolves a registered dependency type.
        /// </summary>
        /// <typeparam name="T">The type of the dependency.</typeparam>
        /// <returns>An instance of the dependency if it could be resolved.</returns>
        public abstract T Resolve<T>();

        /// <summary>
        ///     Resolves a registered dependency type.
        /// </summary>
        /// <param name="serviceType">The type of the dependency.</param>
        /// <returns>An instance of the dependency if it could be resolved.</returns>
        public abstract object Resolve(Type serviceType);

        /// <summary>
        ///     Tries to resolve a registered dependency type.
        /// </summary>
        /// <typeparam name="T">The type of the dependency.</typeparam>
        /// <param name="service">An instance of the dependency if it could be resolved.</param>
        /// <returns>True if the dependency could be resolved, false otherwise.</returns>
        public abstract bool TryResolve<T>(out T service);

        /// <summary>
        ///     Tries to resolve a registered dependency type.
        /// </summary>
        /// <param name="serviceType">The type of the dependency.</param>
        /// <param name="service">An instance of the dependency if it could be resolved.</param>
        /// <returns>True if the dependency could be resolved, false otherwise.</returns>
        public abstract bool TryResolve(Type serviceType, out object service);

        public abstract T GetState<T>(string name);
        public abstract void SetState<T>(string name, T value);
    }
}