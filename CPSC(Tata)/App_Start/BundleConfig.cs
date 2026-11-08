using System.Web;
using System.Web.Optimization;

namespace InputOutput
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // VAPT finding "Vulnerable and Outdated Components": jQuery 1.10.2 (2013) carries
            // unpatched XSS CVEs (CVE-2020-11022/11023) that were never backported to the 1.x line -
            // closing it means the 3.x line. jQuery Migrate loads right after and restores/shims most
            // APIs removed between 1.x and 3.x, logging a console warning instead of a hard failure
            // for anything it can't fully polyfill - a safety net for the ~400 legacy views that were
            // written against 1.x, not a guarantee every one of them needs no further changes.
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-migrate-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Bootstrap 3 (Scripts/bootstrap.js) was removed - Checkmarx SCA CVE-2024-6485 / CVE-2025-1647.
            // The app uses Bootstrap 5 from assets/global/plugins/bootstrap (see bs3-compat.css/js).
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));
        }
    }
}
