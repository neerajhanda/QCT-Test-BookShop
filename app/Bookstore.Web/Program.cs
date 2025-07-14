
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using NLog.Extensions.Logging;

    namespace Bookstore
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add configuration sources
                builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
                builder.Configuration.AddEnvironmentVariables();

// Add connection string - using EF6 connection string configuration
                var connectionString = builder.Configuration.GetConnectionString("BookstoreDatabaseConnection") ??
                    "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=BookStoreClassic;MultipleActiveResultSets=true;Integrated Security=SSPI;";
                System.Data.Entity.Database.SetInitializer<System.Data.Entity.DbContext>(null);

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;
                
                // Configure connection strings - connection string is already set above

                // Add services to the container (formerly ConfigureServices)
                builder.Services.AddControllersWithViews(options => {
                    options.EnableEndpointRouting = false;
                });
                builder.Services.AddRazorPages();

                // Configure client-side validation
                builder.Services.AddMvc()
                    .AddViewOptions(options => {
                        options.HtmlHelperOptions.ClientValidationEnabled = true;
                    });

                // Add logging
                builder.Logging.AddNLog();

                //Added Services


                
                var app = builder.Build();
                
                // Configure the HTTP request pipeline (formerly Configure method)
                if (app.Environment.IsDevelopment())
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

                // Bundle and minification middleware
                if (!app.Environment.IsDevelopment())
                {
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        OnPrepareResponse = context =>
                        {
                            // Cache static files for 1 year
                            context.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000";
                        }
                    });
                }

                //Added Middleware

                app.UseRouting();
                
                app.UseAuthorization();

                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;
                        logger.LogError(exception, "Unhandled exception");

                        context.Response.Redirect("/Home/Error");
                    });
                });

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Register area routes
                app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                app.MapRazorPages();

                
                app.Run();
            }
        }
        
        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }

            public static string GetAppSetting(string key)
            {
                return Configuration[$"AppSettings:{key}"];
            }

            public static bool GetAppSettingBool(string key, bool defaultValue = false)
            {
                string value = GetAppSetting(key);
                if (bool.TryParse(value, out bool result))
                {
                    return result;
                }
                return defaultValue;
            }
        }
    }