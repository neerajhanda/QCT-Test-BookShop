using Microsoft.Owin;
using Owin;
using Bookstore.WebForms;

[assembly: OwinStartup(typeof(Startup))]

namespace Bookstore.WebForms
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure dependency injection
            DependencyInjectionSetup.ConfigureDependencyInjection(app);
            
            // Configure authentication
            AuthenticationSetup.ConfigureAuthentication(app);
        }
    }
}