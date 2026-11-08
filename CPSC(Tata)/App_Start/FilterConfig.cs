using System.Web;
using System.Web.Mvc;

namespace InputOutput
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Was HandleErrorAttribute() - see LoggingHandleErrorAttribute for why plain
            // HandleError was silently swallowing every action-level exception unlogged.
            filters.Add(new LoggingHandleErrorAttribute());
            filters.Add(new NoCacheAttribute());
            filters.Add(new SingleSessionAttribute());
            // Checkmarx "CSRF": every POST must carry a valid anti-forgery token (see
            // AntiForgeryOnPostAttribute.cs and Scripts/csrf-guard.js).
            filters.Add(new AntiForgeryOnPostAttribute());
        }
    }
}
