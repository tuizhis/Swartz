using System.Collections.Generic;
using System.Web.Mvc;
using IFilterProvider = Swartz.Mvc.Filters.IFilterProvider;

namespace Swartz.Data.Filters
{
    public class TransactionFilter : ActionFilterAttribute, IFilterProvider
    {
        private readonly IEnumerable<ITransactionManager> _transactionManagers;

        public TransactionFilter(IEnumerable<ITransactionManager> transactionManagers)
        {
            _transactionManagers = transactionManagers;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            foreach (var manager in _transactionManagers)
            {
                manager.Save();
            }
        }
    }
}