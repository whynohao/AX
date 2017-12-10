using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Ax.Server
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
             name: "Video",
             url: "{controller}/{action}.aspx/{id}",
             defaults: new { controller = "Video", action = "VideoShow", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Upgrade",
                url: "update/",
                defaults: new { controller = "ServerManager", action = "Upgrade", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "Register",
               url: "register/",
               defaults: new { controller = "ServerManager", action = "Register", id = UrlParameter.Optional }
            );
            routes.MapRoute(
              name: "RecoverPassword",
              url: "recoverPassword/",
              defaults: new { controller = "ServerManager", action = "RecoverPassword", id = UrlParameter.Optional }
            );
            routes.MapRoute(
             name: "Document",
             url: "document/{action}",
             defaults: new { controller = "Document", action = "ReadOnly", id = UrlParameter.Optional }
            );
            routes.MapRoute(
             name: "Desk",
             url: "Desk/{action}",
             defaults: new { controller = "Desk", action = "ReadOnly", id = UrlParameter.Optional }
            );
            routes.MapRoute(
             name: "CpsModule",
             url: "CpsModule/{action}",
             defaults: new { controller = "CpsModule", action = "CheckIcon", id = UrlParameter.Optional }
            );
        }
    }
}