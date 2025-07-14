using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;
using Owin;



[assembly: OwinStartup(typeof(Bookstore.Web.Startup))]

namespace Bookstore.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Add LoggingSetup class implementation here since it doesn't exist
            ConfigureLogging();

            // Assuming similar implementation is needed
            ConfigureConfiguration();

            ConfigureDependencyInjection(app);

            ConfigureAuthentication(app);
        }

        // Added these methods to replace the missing classes
        private void ConfigureLogging()
        {
            // Add logging configuration implementation here
        }

        private void ConfigureConfiguration()
        {
            // Add configuration implementation here
        }

        private void ConfigureDependencyInjection(IAppBuilder app)
        {
            // Add dependency injection configuration here
        }

        private void ConfigureAuthentication(IAppBuilder app)
        {
            // Add authentication configuration here
        }
    }
}