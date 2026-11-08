using System;
using System.Web.Helpers;
using System.Web.Mvc;

namespace InputOutput
{
    // Checkmarx "CSRF" (CWE-352) remediation - server half, registered globally in FilterConfig.
    //
    // Every POST to an MVC action must carry a valid anti-forgery token, either as the standard
    // __RequestVerificationToken form field (forms / url-encoded AJAX) or the RequestVerificationToken
    // header (Scripts/csrf-guard.js adds both automatically). The cookie half is issued by
    // @Html.AntiForgeryToken(), which _Layout and the stand-alone pages render once per page.
    //
    // Individual state-changing actions additionally carry the built-in [ValidateAntiForgeryToken]
    // so the protection is visible at the action itself. Actions that legitimately receive POSTs
    // from outside this application can opt out with [SkipAntiForgery].
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class AntiForgeryOnPostAttribute : FilterAttribute, IAuthorizationFilter
    {
        public const string HeaderName = "RequestVerificationToken";

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");

            var request = filterContext.HttpContext.Request;
            if (!string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) return;

            if (filterContext.ActionDescriptor.IsDefined(typeof(SkipAntiForgeryAttribute), true)
                || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(SkipAntiForgeryAttribute), true))
            {
                return;
            }

            var cookie = request.Cookies[AntiForgeryConfig.CookieName];
            string cookieToken = cookie != null ? cookie.Value : null;
            string formToken = request.Form["__RequestVerificationToken"];
            if (string.IsNullOrEmpty(formToken))
            {
                formToken = request.Headers[HeaderName];
            }

            // Throws HttpAntiForgeryException (handled by the normal error pipeline) on a missing or
            // mismatched token.
            AntiForgery.Validate(cookieToken, formToken);
        }
    }

    // Opt-out marker for the global AntiForgeryOnPostAttribute (e.g. an identity-provider callback
    // that is POSTed to by another origin and is protected by its own state/nonce instead).
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class SkipAntiForgeryAttribute : Attribute
    {
    }
}
