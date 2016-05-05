using System.Web;

namespace Swartz.Mvc
{
    public interface IHttpContextAccessor
    {
        HttpContextBase Current();

        void Set(HttpContextBase httpContext);
    }
}