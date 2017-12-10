using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Jikon.AX.APPPushService.Startup))]
namespace Jikon.AX.APPPushService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
