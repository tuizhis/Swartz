﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Swartz.WebApi.Filters
{
    public class SwartzApiExceptionFilterDispatcher : IExceptionFilter
    {
        public bool AllowMultiple { get; }

        public async Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext,
            CancellationToken cancellationToken)
        {
            var workContext = actionExecutedContext.ActionContext.ControllerContext.GetWorkContext();

            foreach (
                var exceptionFilter in workContext.Resolve<IEnumerable<IApiFilterProvider>>().OfType<IExceptionFilter>()
                )
            {
                await exceptionFilter.ExecuteExceptionFilterAsync(actionExecutedContext, cancellationToken);
            }
        }
    }
}