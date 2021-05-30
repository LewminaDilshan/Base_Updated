using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RedLineLanka_Enterprise.Startup))]
namespace RedLineLanka_Enterprise
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
