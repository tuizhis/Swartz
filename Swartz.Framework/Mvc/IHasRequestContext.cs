using System.Web.Routing;

namespace Swartz.Mvc
{
    public interface IHasRequestContext
    {
        RequestContext RequestContext { get; }
    }
}