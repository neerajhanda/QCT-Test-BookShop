using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;
using Owin;
using NLog;
using NLog.Extensions.Logging;

[assembly: OwinStartup(typeof(Bookstore.Web.Startup))]

namespace Bookstore.Web
{
    public static class AuthenticationConfig
    {
        public static void ConfigureAuthentication(IAppBuilder app)
        {
            // Configure authentication
        }
    }

    public static class LoggingSetup
    {
        public static void ConfigureLogging()
        {
            // Initialize NLog
            LogManager.LoadConfiguration("NLog.config");
        }
    }

    public static class ConfigurationSetup
    {
        public static void ConfigureConfiguration()
        {
            // Configure application settings
        }
    }

    public static class DependencyInjectionSetup
    {
        public static void ConfigureDependencyInjection(IAppBuilder app)
        {
            // Configure dependency injection
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LoggingSetup.ConfigureLogging();

            ConfigurationSetup.ConfigureConfiguration();

            DependencyInjectionSetup.ConfigureDependencyInjection(app);

            AuthenticationConfig.ConfigureAuthentication(app);
        }
    }
}