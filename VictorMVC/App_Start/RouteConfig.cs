using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace VictorMVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Articles",
                url: "Admin/Articles/{action}/{id}",
                defaults: new { controller = "Articles", action = "Manage", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Demos",
                url: "Admin/Demos/{action}/{id}",
                defaults: new { controller = "Demos", action = "Manage", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "EmailRecipients",
                url: "Admin/EmailRecipients/{action}/{id}",
                defaults: new { controller = "EmailRecipients", action = "Manage", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Smtp",
                url: "Admin/Smtp/{action}/{id}",
                defaults: new { controller = "Smtp", action = "Modify", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Admin",
                url: "Admin/{action}/{id}",
                defaults: new { controller = "Admin", action = "Menu", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Default",
                url: "{action}/{id}",
                defaults: new { controller = "Public", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
