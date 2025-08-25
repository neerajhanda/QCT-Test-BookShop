
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using EntityFramework = System.Data.Entity;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

    namespace Bookstore.Web
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add connection string from Web.config
                builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                builder.Configuration.AddEnvironmentVariables();

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;

                // Add services to the container (formerly ConfigureServices)
                builder.Services.AddControllersWithViews(options => {
                    // Add MVC filters here if needed (equivalent to FilterConfig.RegisterGlobalFilters)
                    options.Filters.Add(typeof(CustomHandleErrorAttribute));
                });

                // Entity Framework 6.x is already registered via package reference
                // No need to call AddEntityFramework() as it doesn't exist in .NET Core/.NET 8

                // Add client-side validation
                builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

                // Add service configurations from appSettings
                ConfigureServices(builder.Services, builder.Configuration);

                // Add logging
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
                builder.Logging.AddDebug();

                var app = builder.Build();
                
                // Configure the HTTP request pipeline (formerly Configure method)
                string environment = builder.Configuration["Environment"] ?? "Development";
                if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                // Register areas (equivalent to AreaRegistration.RegisterAllAreas())
                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();
                
                app.UseEndpoints(endpoints =>
                {
                    // Set up routes (equivalent to RouteConfig.RegisterRoutes)
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");

                    // Add additional routes as needed
                    endpoints.MapControllerRoute(
                        name: "areas",
                        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                });

                // Configure global error handling
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        if (exception != null)
                        {
                            logger.LogError(exception, "An unhandled exception occurred");
                        }

                        await Task.CompletedTask;
                    });
                });

                app.Run();
            }

            private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
                // Configure services based on appSettings from Web.config
                string authService = configuration["Services/Authentication"] ?? "local";
                string dbService = configuration["Services/Database"] ?? "local";
                string fileService = configuration["Services/FileService"] ?? "local";
                string imageValidationService = configuration["Services/ImageValidationService"] ?? "local";
                string loggingService = configuration["Services/LoggingService"] ?? "local";

                // Add any service configurations based on the settings
                if (authService == "aws")
                {
                    // Configure AWS authentication if needed
                    // This would use the Cognito settings from Web.config
                }

                if (fileService == "aws")
                {
                    // Configure AWS file services if needed
                    // This would use the S3 bucket and CloudFront settings from Web.config
                }
            }
        }

        /// <summary>
        /// Static configuration manager to provide access to configuration throughout the application
        /// </summary>
        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }
        }

        /// <summary>
        /// Classes that would typically be defined in separate files in ASP.NET Core
        /// These would normally be in separate files in the App_Start folder in ASP.NET Framework
        /// </summary>
        // These are kept for compatibility with existing code references
        // but the actual implementation is now in Program.cs
        public static class FilterConfig
        {
            public static void RegisterGlobalFilters(GlobalFilterCollection filters)
            {
                filters.Add(new CustomHandleErrorAttribute());
            }
        }

        public static class RouteConfig
        {
            public static void RegisterRoutes(RouteCollection routes)
            {
                RouteCollection.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}"
                );
            }
        }

        public static class BundleConfig
        {
            public static void RegisterBundles(BundleCollection bundles)
            {
// In ASP.NET Core, bundling and minification is typically done using tools like webpack, gulp, etc.
                // This method is kept for reference but won't be used
            }
        }

        // These classes are needed for compatibility with the old code
        public class GlobalFilterCollection : List<object> { }
        public class RouteCollection { public static void MapRoute(string name, string template) { } }
        public class BundleCollection { }

        // Custom error handler attribute for ASP.NET Core
        public class CustomHandleErrorAttribute : ExceptionFilterAttribute
        {
            public override void OnException(ExceptionContext context)
            {
                // Default error handling logic
                if (context.ExceptionHandled == false)
                {
                    // Log the error
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<CustomHandleErrorAttribute>>();
                    logger?.LogError(context.Exception, "An unhandled exception occurred");

                    // You can redirect to an error page or return a specific view
                    // For now, we'll just mark it as handled
                    context.ExceptionHandled = true;
                }

                base.OnException(context);
            }
        }

        // Keep this for backward compatibility with code references
        public class HandleErrorAttribute : Attribute { }
    }