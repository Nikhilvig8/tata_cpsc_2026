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
            MvcHandler.DisableMvcResponseHeader = true;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
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
