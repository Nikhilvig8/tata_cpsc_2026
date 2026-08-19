using System.Web.Mvc;
using System.Web.Security;

namespace InputOutput
{
    // VAPT finding "Concurrent login allowed" (see ConcurrentSessionGuard for the storage/logic).
    // Registered as a global filter in FilterConfig, so it runs on every action - but only acts once
    // Session["Uid"] is set (i.e. a completed login), so it never interferes with the Login
    // controller's own pre-login actions (Login, LoginCheck, CaptchaImage, VerifyMfa*, SetupMfa*,
    // Sso*, Logout).
    public class SingleSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session != null && session["Uid"] != null)
            {
                string username = session["ConcurrentSessionUser"] as string;
                string token = session["ActiveLoginToken"] as string;

                if (string.IsNullOrEmpty(token))
                {
                    // Session predates this feature (or survived an app-pool recycle that wiped
                    // the guard's cache but not this in-proc Session) - adopt it now instead of
                    // rejecting a legitimately logged-in user with nothing yet to compare against.
                    if (!string.IsNullOrEmpty(username))
                    {
                        session["ActiveLoginToken"] = ConcurrentSessionGuard.Establish(username);
                    }
                }
                else if (!ConcurrentSessionGuard.Touch(username, token))
                {
                    // Clear (not Abandon) deliberately: same pattern LoginCheck's failure branches
                    // use, so Session["Popup"] set below survives into the redirected request on
                    // the same session cookie for Login.cshtml to read.
                    session.Clear();
                    FormsAuthentication.SignOut();
                    session["Popup"] = "6";
                    filterContext.Result = new RedirectResult("~/Login/Login");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
