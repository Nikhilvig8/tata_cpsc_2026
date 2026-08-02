using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CPSC_Tata_.Startup))]
namespace CPSC_Tata_
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
