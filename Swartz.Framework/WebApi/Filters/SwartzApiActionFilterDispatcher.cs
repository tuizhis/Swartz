using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Swartz.WebApi.Filters
{
    public class SwartzApiActionFilterDispatcher : IActionFilter
    {
        public bool AllowMultiple { get; }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext,
            CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var workContext = actionContext.ControllerContext.GetWorkContext();

            foreach (var actionFilter in workContext.Resolve<IEnumerable<IApiFilterProvider>>().OfType<IActionFilter>())
            {
                var tempContinuation = continuation;
                continuation =
                    () => actionFilter.ExecuteActionFilterAsync(actionContext, cancellationToken, tempContinuation);
            }

            return await continuation();
        }
    }
}