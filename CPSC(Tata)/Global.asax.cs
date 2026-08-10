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
        // execution, or view rendering. Writes to a file under the site's own folder so it's
        // readable without server/Event-Viewer access. Remove once the root cause is found.
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("\r\n===== " + DateTime.Now + " =====");
                sb.AppendLine("URL: " + Request.Url);
                Exception current = ex;
                while (current != null)
                {
                    sb.AppendLine(current.GetType().FullName + ": " + current.Message);
                    sb.AppendLine(current.StackTrace);
                    current = current.InnerException;
                    if (current != null) sb.AppendLine("--- Inner Exception ---");
                }
                File.AppendAllText(Server.MapPath("~/Logs/AppErrors.txt"), sb.ToString());
            }
            catch
            {
                // Logging must never itself throw and add a third exception to the pile.
            }
        }
    }
}
