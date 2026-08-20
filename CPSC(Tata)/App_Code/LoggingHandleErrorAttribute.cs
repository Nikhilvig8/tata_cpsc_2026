using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace InputOutput
{
    // Registered globally in FilterConfig.cs in place of a bare HandleErrorAttribute.
    //
    // HandleErrorAttribute sets ExceptionContext.ExceptionHandled = true before rendering the
    // generic "Error." view - which means the exception is considered handled by MVC's own
    // pipeline and never reaches Global.asax's Application_Error, no matter how many fallback
    // paths that method tries. Every controller in this app already carries a bare [HandleError()]
    // (redundant with the global registration, but same effect) - so no action-level exception has
    // ever actually been logged anywhere, despite the app appearing to have robust error logging.
    // This subclass logs first (same file + fallback-path pattern Application_Error already uses,
    // so anyone already checking those files finds MVC-level errors there too), then defers to the
    // normal base behavior so the user-facing page is unchanged.
    public class LoggingHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.Exception != null && !filterContext.ExceptionHandled)
            {
                LogException(filterContext);
            }

            base.OnException(filterContext);
        }

        private static void LogException(ExceptionContext filterContext)
        {
            string message;
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("\r\n===== [MVC-UNHANDLED] " + DateTime.Now + " =====");
                try
                {
                    sb.AppendLine("Controller: " + filterContext.RouteData.Values["controller"]);
                    sb.AppendLine("Action: " + filterContext.RouteData.Values["action"]);
                    sb.AppendLine("URL: " + filterContext.HttpContext.Request.Url);
                }
                catch (Exception metaEx)
                {
                    sb.AppendLine("<failed to read request metadata: " + metaEx.Message + ">");
                }

                Exception current = filterContext.Exception;
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
                message = "\r\n===== [MVC-UNHANDLED] " + DateTime.Now + " ===== (failed to read the real exception: " + outerEx + ")";
            }

            try { System.Diagnostics.EventLog.WriteEntry(".NET Runtime", message, System.Diagnostics.EventLogEntryType.Error); }
            catch { }

            try { File.AppendAllText(@"C:\AppErrors_Fallback.txt", message); }
            catch { }

            try { File.AppendAllText(@"D:\AppErrors_Fallback.txt", message); }
            catch { }

            try { File.AppendAllText(HttpContext.Current.Server.MapPath("~/Logs/AppErrors.txt"), message); }
            catch { }
        }
    }
}
