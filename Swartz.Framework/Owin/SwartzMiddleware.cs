using System;
using System.Threading.Tasks;
using Owin;

namespace Swartz.Owin
{
    /// <summary>
    /// A special Owin middleware that is executed last in the Owin pipeline and runs the non-Owin part of the request.
    /// </summary>
    public static class SwartzMiddleware
    {
        public static IAppBuilder UseSwartz(this IAppBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var handler = context.Environment["swartz.Handler"] as Func<Task>;

                if (handler == null)
                {
                    throw new ArgumentException("swartz.Handler can't be null");
                }

                await handler();
            });

            return app;
        }
    }
}