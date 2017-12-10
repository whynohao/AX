using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Jikon.AX.APPPushService.ChannelProvider;
using Jikon.AX.APPPushService.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Jikon.AX.APPPushService
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            EnvProvider.Default.Init();//初始化配置信息
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            #region AutoFac
            //----AutoFac  DI------
            var builder = new ContainerBuilder();
            SetupResolveRules(builder);
            //RegisterApiControllers方法
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            //RegisterControllers方法 
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            //注意此处HttpConfiguration类的 config对象，一定不要new,要从GlobalConfiguration获取
            HttpConfiguration config = GlobalConfiguration.Configuration;
            //注意此处与MVC依赖注入不同
            config.DependencyResolver = (new AutofacWebApiDependencyResolver(container));
            //--------

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            #endregion
        }
        /// <summary>
        /// 设置Autofac自定解析依赖的规则
        /// </summary>
        /// <param name="builder"></param>
        private static void SetupResolveRules(ContainerBuilder builder)
        {
            //WebAPI只用引用provider接口，不使用具体实现。
            //如需加载实现的程序集，将dll拷贝到bin目录下即可，不用引用dll

            var channelProvider = Assembly.GetExecutingAssembly();
            builder.RegisterType<ProviderGetter>().As<IProviderGetter>();//注册通道提供器的获取器，实现以名称（配置中的名称）区分不同的实现

            ////根据名称约定（通道提供者的接口和实现均以Provider结尾），实现接口和实现的依赖
            //builder.RegisterAssemblyTypes(channelProvider)
            //  .Where(t => t.Name.EndsWith("Provider"))
            //  .AsImplementedInterfaces();

            //注册相同接口的多个实现，以不同的名称做区分
            builder.RegisterType<GetuiProvider>().Named<IChannelProvider>(PushChannelType.Getui.ToString());
            builder.RegisterType<AlibabaProvider>().Named<IChannelProvider>(PushChannelType.Alibaba.ToString());
        }
    }
}
