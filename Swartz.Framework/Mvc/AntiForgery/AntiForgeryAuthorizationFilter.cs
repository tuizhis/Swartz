using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using IFilterProvider = Swartz.Mvc.Filters.IFilterProvider;

namespace Swartz.Mvc.AntiForgery
{
    public class AntiForgeryAuthorizationFilter : IFilterProvider, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // If the request is not a POST or is anonymous, and the request doesn't have validation forced, return.
            if (filterContext.HttpContext.Request.HttpMethod != "POST" && !ShouldValidateGet(filterContext))
            {
                return;
            }

            if (!IsAntiForgeryProtectionEnabled(filterContext))
            {
                return;
            }

            var validator = new ValidateAntiForgeryTokenAttribute();
            validator.OnAuthorization(filterContext);

            if (filterContext.HttpContext is HackHttpContext)
                filterContext.HttpContext = ((HackHttpContext) filterContext.HttpContext).OriginalHttpContextBase;
        }

        private bool IsAntiForgeryProtectionEnabled(AuthorizationContext context)
        {
            // POST is opt-out
            var attributes =
                (ValidateAntiForgeryTokenSwartzAttribute[])
                    context.ActionDescriptor.GetCustomAttributes(typeof(ValidateAntiForgeryTokenSwartzAttribute), false);

            if (attributes.Length > 0 && !attributes[0].Enabled) return false;

            return true;
        }

        private static bool ShouldValidateGet(AuthorizationContext context)
        {
            const string tokenFieldName = "__RequestVerificationToken";

            var attributes =
                (ValidateAntiForgeryTokenSwartzAttribute[])
                    context.ActionDescriptor.GetCustomAttributes(typeof(ValidateAntiForgeryTokenSwartzAttribute), false);

            if (attributes.Length > 0 && attributes[0].Enabled)
            {
                var request = context.HttpContext.Request;

                //HAACK: (erikpo) If the token is in the querystring, put it in the form so MVC can validate it
                if (!string.IsNullOrEmpty(request.QueryString[tokenFieldName]))
                {
                    context.HttpContext = new HackHttpContext(context.HttpContext,
                        (HttpContext) context.HttpContext.Items["originalHttpContext"]);
                    ((HackHttpRequest) context.HttpContext.Request).AddFormValue(tokenFieldName,
                        context.HttpContext.Request.QueryString[tokenFieldName]);
                }

                return true;
            }

            return false;
        }

        #region HackHttpContext

        private class HackHttpContext : HttpContextWrapper
        {
            private readonly HttpContext _originalHttpContext;
            private HttpRequestWrapper _request;

            public HackHttpContext(HttpContextBase httpContextBase, HttpContext httpContext)
                : base(httpContext)
            {
                OriginalHttpContextBase = httpContextBase;
                _originalHttpContext = httpContext;
            }

            public HttpContextBase OriginalHttpContextBase { get; }

            public override HttpRequestBase Request
                => _request ?? (_request = new HackHttpRequest(_originalHttpContext.Request));
        }

        #endregion

        #region HackHttpRequest

        private class HackHttpRequest : HttpRequestWrapper
        {
            private readonly HttpRequest _originalHttpRequest;
            private NameValueCollection _form;

            public HackHttpRequest(HttpRequest httpRequest)
                : base(httpRequest)
            {
                _originalHttpRequest = httpRequest;
            }

            public override NameValueCollection Form
                => _form ?? (_form = new NameValueCollection(_originalHttpRequest.Form));

            public void AddFormValue(string key, string value)
            {
                Form.Add(key, value);
            }
        }

        #endregion
    }
}