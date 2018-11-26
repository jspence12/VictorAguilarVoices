using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace VictorMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var razorEngine = ViewEngines.Engines.OfType<RazorViewEngine>().FirstOrDefault();
            razorEngine.ViewLocationFormats = razorEngine.ViewLocationFormats.Concat(new string[]
            {
                "~/Views/Admin/{1}/{0}.cshtml",
                "~/Views/Admin/{0}.cshtml"
            }).ToArray();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError().GetBaseException();
            if (exc.GetType() == typeof(HttpException))
            {
                HttpException http = (HttpException)exc;
                switch (http.GetHttpCode())
                {
                    case 401:
                        Response.Clear();
                        Response.Redirect("~/Admin/Login");
                        Response.End();
                        return;
                    default:
                        break;
                }
            }
        }
    }
}

