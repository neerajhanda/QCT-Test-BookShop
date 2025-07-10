using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;
using Owin;


[assembly: OwinStartup(typeof(Bookstore.Web.Startup))]

namespace Bookstore.Web
{
    public class ConfigurationSetup
    {
        public static void ConfigureConfiguration()
        {
            // Add configuration setup logic here
        }
    }

    public class DependencyInjectionSetup
    {
        public static void ConfigureDependencyInjection(IAppBuilder app)
        {
            // Add dependency injection setup logic here
        }
    }

    public class AuthenticationConfig
    {
        public static void ConfigureAuthentication(IAppBuilder app)
        {
            // Add authentication configuration logic here
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure logging directly
            ConfigureLogging();

            ConfigurationSetup.ConfigureConfiguration();

            DependencyInjectionSetup.ConfigureDependencyInjection(app);

            AuthenticationConfig.ConfigureAuthentication(app);
        }

        private void ConfigureLogging()
        {
            // Configure NLog or other logging
            NLog.LogManager.Configuration = new NLog.Config.LoggingConfiguration();
            // Add additional logging configuration as needed
        }
    }
}