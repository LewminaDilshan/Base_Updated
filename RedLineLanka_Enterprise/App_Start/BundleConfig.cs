using System.Web;
using System.Web.Optimization;

namespace RedLineLanka_Enterprise
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region JavaScripts

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      //"~/Scripts/bootstrap.bundle.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                      "~/Scripts/jquery-ui-{version}.js",
                      "~/Scripts/jquery-ui-timepicker-addon.js",
                      "~/Scripts/jquery-ui-MonthPicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include(
                      "~/Scripts/site.js",
                      "~/Scripts/PopUpSelector.js",
                      "~/Scripts/fontawesome/all.js"));

            bundles.Add(new ScriptBundle("~/bundles/chart").Include(
                      "~/Scripts/Chart.js"));

            bundles.Add(new ScriptBundle("~/bundles/layout").IncludeDirectory(
                      "~/Scripts/Layout", "*.js", false));

            bundles.Add(new ScriptBundle("~/bundles/layoutless").Include(
                "~/Scripts/Layout/jquery.autosize.js",
                "~/Scripts/Layout/jquery.customSelect.js",
                "~/Scripts/Layout/jquery.nicescroll.js",
                "~/Scripts/Layout/jquery.placeholder.js",
                "~/Scripts/Layout/jquery.scrollTo.js",
                "~/Scripts/Layout/jquery.slimscroll.js"));

            bundles.Add(new StyleBundle("~/Scripts/bootstrapbundleminjs").Include(
                      "~/Scripts/bootstrap.bundle.min.js"));

            #endregion

            #region View Specific JS

            bundles.Add(new ScriptBundle("~/bundles/admin/user").Include(
                      "~/Scripts/Admin/User.js"));

            bundles.Add(new ScriptBundle("~/bundles/admin/userrole").Include(
                      "~/Scripts/Admin/UserRole.js"));
            #endregion

            #region Styles
            bundles.Add(new StyleBundle("~/Content/steps").Include(
                 "~/Content/steps.css"));

            bundles.Add(new StyleBundle("~/Content/jqueryuitimepicker").Include(
                      "~/Content/jquery-ui-timepicker-addon.css",
                      "~/Content/jquery-ui-MonthPicker.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                      "~/Content/bootstrap.css"));

            bundles.Add(new StyleBundle("~/Content/custom").Include(
                      "~/Content/Site.css",
                      "~/Content/PopUpSelector.css"));

            bundles.Add(new StyleBundle("~/Content/fontawesome").Include(
                      "~/Content/fontawesome-all.css"));

            bundles.Add(new StyleBundle("~/Content/Layout/LayoutBundle").IncludeDirectory(
                      "~/Content/Layout", "*.css"));
            bundles.Add(new StyleBundle("~/Content/bootstrapicons").Include(
                      "~/Content/bootstrap-icons.css"));
            bundles.Add(new StyleBundle("~/Content/bootstrapmin").Include(
                      "~/Content/bootstrap.min.css"));
            #endregion
        }
    }
}
