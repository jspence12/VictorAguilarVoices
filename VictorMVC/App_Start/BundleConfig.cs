using System.Web;
using System.Web.Optimization;

namespace VictorMVC
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;
            bundles.IgnoreList.Clear();
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.min.js", 
                        "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/summernote").Include(
                      "~/Scripts/summernote-bs4.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/modify").Include(
                      "~/Scripts/modify.js"));
            bundles.Add(new ScriptBundle("~/bundles/manage").Include(
                      "~/Scripts/manage.js"));
            bundles.Add(new StyleBundle("~/content/bundles/css").Include(
                      "~/Content/Css/bootstrap.min.css",
                      "~/Content/Css/font-awesome.min.css",
                      "~/Content/Css/site.css"));
            bundles.Add(new StyleBundle("~/content/bundles/css/summernote").Include(
                "~/Content/Css/summernote-bs4.css"));
        }
    }
}
