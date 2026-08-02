using System;
using System.Web;
using System.Web.Mvc;

namespace InputOutput
{
    // Mitigates the "Back/Refresh after logout still shows the page" finding: tells the
    // browser never to serve a cached copy of any MVC-rendered page from its history/back-forward
    // cache. Registered as a global filter in FilterConfig, so it applies to every action.
    // Static assets (css/js/images under Content, Scripts, assets) are served by IIS directly,
    // not through MVC, so their caching is unaffected.
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            HttpResponseBase response = filterContext.HttpContext.Response;
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.Cache.SetValidUntilExpires(false);
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Headers.Remove("Pragma");
            response.Headers.Add("Pragma", "no-cache");

            base.OnResultExecuting(filterContext);
        }
    }
}
