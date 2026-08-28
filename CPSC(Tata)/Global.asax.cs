using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InputOutput
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // .NET Framework 4.5.2 defaults ServicePointManager.SecurityProtocol to Ssl3/Tls
            // (TLS 1.0) only - confirmed on the production server via
            // [System.Net.ServicePointManager]::SecurityProtocol returning "Ssl3, Tls". Every
            // outbound HTTPS call this app makes to Keycloak (sso.tatamotors.com, fronted by
            // Cloudflare) needs TLS 1.2: Cloudflare rejects SSL3/TLS1.0 handshakes outright, which
            // is what was surfacing as IsValidOID1's "Local Security Authority cannot be
            // contacted" crash (AppErrors.txt, 2026-08-28 19:45:08) - the TLS handshake fails
            // before Keycloak is ever reached, not a network/firewall issue (TCP connect and ping
            // to sso.tatamotors.com both succeed).
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            MvcHandler.DisableMvcResponseHeader = true;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // VAPT finding "Secure Cookies not set properly": Web.config's <httpCookies
        // httpOnlyCookies="true" requireSSL="true" /> already covers HttpOnly + Secure for every
        // cookie this app sets (session + forms auth). SameSite is the piece that config element
        // can't express on .NET Framework 4.6 (that attribute wasn't added to HttpCookie/web.config
        // until later 4.7.2 patches) - so it's added the standard way for this framework version, by
        // appending it to the cookie's Path here right before the response headers are written. Lax
        // (not Strict) so a link from an external page can still land the user on an authenticated
        // page instead of silently dropping the session cookie; the site is 100% same-site POST
        // forms otherwise, so this closes CSRF-via-cookie exposure without breaking normal use.
        protected void Application_PreSendRequestHeaders()
        {
            HttpResponse response = Response;
            if (response == null) return;

            foreach (string name in response.Cookies.AllKeys)
            {
                HttpCookie cookie = response.Cookies[name];
                if (cookie != null && cookie.Path.IndexOf("SameSite", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    cookie.Path += "; SameSite=Lax";
                }
            }
        }

        // TEMPORARY: diagnosing the "Runtime Error - exception occurred while executing the
        // custom error page" seen in production. Application_Error is the true global catch-all -
        // it fires for the original unhandled exception before customErrors' redirect even
        // happens, regardless of whether the failure is in routing, an HTTP module, MVC action
        // execution, or view rendering.
        //
        // Hardened with multiple independent, isolated fallbacks (event log + two hardcoded
        // absolute paths, alongside the original app-relative one) after a first version using
        // only Server.MapPath("~/Logs/...") produced no output in production despite the app
        // demonstrably reaching this far (the customErrors redirect page did render) - Request.Url
        // or Server.MapPath themselves throwing (HttpContext not fully populated yet for whatever
        // triggered the original exception) was the leading suspect, so each write attempt below
        // is wrapped separately and none can suppress another. Remove all of this once the root
        // cause is found.
        protected void Application_Error(object sender, EventArgs e)
        {
            string message;
            try
            {
                Exception ex = Server.GetLastError();
                var sb = new StringBuilder();
                sb.AppendLine("\r\n===== " + DateTime.Now + " =====");
                try { sb.AppendLine("URL: " + Request.Url); }
                catch (Exception urlEx) { sb.AppendLine("URL: <unavailable: " + urlEx.Message + ">"); }
                Exception current = ex;
                while (current != null)
                {
                    sb.AppendLine(current.GetType().FullName + ": " + current.Message);
                    sb.AppendLine(current.StackTrace);
                    current = current.InnerException;
                    if (current != null) sb.AppendLine("--- Inner Exception ---");
                }
                message = sb.ToString();
            }
            catch (Exception outerEx)
            {
                message = "\r\n===== " + DateTime.Now + " ===== (failed to read the real exception: " + outerEx + ")";
            }

            try { System.Diagnostics.EventLog.WriteEntry(".NET Runtime", "[AppErrors] " + message, System.Diagnostics.EventLogEntryType.Error); }
            catch { }

            try { File.AppendAllText(@"C:\AppErrors_Fallback.txt", message); }
            catch { }

            try { File.AppendAllText(@"D:\AppErrors_Fallback.txt", message); }
            catch { }

            try { File.AppendAllText(Server.MapPath("~/Logs/AppErrors.txt"), message); }
            catch { }
        }
    }
}
