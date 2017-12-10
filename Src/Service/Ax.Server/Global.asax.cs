using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AxCRL.Services;
using Ax.Server.Models.Bcf;

namespace Ax.Server
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AxServiceBus bus = new AxServiceBus();
            bus.Start();
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configuration.EnableCors();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            //#region 启用调用排程
            //SystemManager system = new SystemManager();
            //system.OpenScheduleTask();

            //#endregion

            APPCache.SetAPPCache(string.Empty);
            APPCache.RemoveAPPCache(string.Empty);
        }
    }
}