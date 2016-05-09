using System.Web.Mvc;
using IFilterProvider = Swartz.Mvc.Filters.IFilterProvider;

namespace Swartz.Data.Filters
{
    public class TransactionFilter : ActionFilterAttribute, IFilterProvider
    {
        private readonly ITransactionManager _transactionManager;

        public TransactionFilter(ITransactionManager transactionManager)
        {
            _transactionManager = transactionManager;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            _transactionManager.Save();
        }
    }
}