using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcWebRole.Startup))]
namespace MvcWebRole
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
