﻿using System;
using System.Web.Mvc;
using Swartz.Utility.Extensions;

namespace Swartz.Mvc.Extensions
{
    public static class ControllerExtensions
    {
        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl,
            Func<ActionResult> invalidUrlBehavior)
        {
            if (!string.IsNullOrWhiteSpace(redirectUrl) && controller.Request.IsLocalUrl(redirectUrl))
            {
                return new RedirectResult(redirectUrl);
            }
            return invalidUrlBehavior?.Invoke();
        }

        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl)
        {
            return RedirectLocal(controller, redirectUrl, (string) null);
        }

        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl, string defaultUrl)
        {
            if (controller.Request.IsLocalUrl(redirectUrl))
            {
                return new RedirectResult(redirectUrl);
            }

            return new RedirectResult(defaultUrl ?? "~/");
        }
    }
}