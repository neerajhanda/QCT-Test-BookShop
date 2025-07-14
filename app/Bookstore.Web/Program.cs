
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
using NLog.Extensions.Logging;
using System.Data.Entity;
using System.Data.SqlClient;

namespace Bookstore
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add connection string from web.config
                builder.Configuration.GetConnectionString("BookstoreDatabaseConnection");

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;
                
// Add connection strings from web.config - using EF6 approach
                builder.Services.AddScoped<DbContext>(serviceProvider => {
                    // Use the same connection string from web.config
                    var connectionString = builder.Configuration.GetConnectionString("BookstoreDatabaseConnection");
                    // Return a new instance of your EF6 DbContext
                    // Note: Replace with your actual DbContext implementation if available
                    return new DbContext(connectionString);
                });

                // Configure client validation settings
                builder.Services.AddControllersWithViews(options => {
                    options.EnableEndpointRouting = false;
                })
                .AddViewOptions(options => {
                    options.HtmlHelperOptions.ClientValidationEnabled = true;
                });

                // Register areas if they exist
                builder.Services.AddMvc().AddControllersAsServices();

                // Configure logging with NLog
                builder.Logging.AddNLog();

                // Register app settings
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Environment", builder.Configuration["Environment"] ?? "Development" },
                    { "Services:Authentication", builder.Configuration["Services:Authentication"] ?? "local" },
                    { "Services:Database", builder.Configuration["Services:Database"] ?? "local" },
                    { "Services:FileService", builder.Configuration["Services:FileService"] ?? "local" },
                    { "Services:ImageValidationService", builder.Configuration["Services:ImageValidationService"] ?? "local" },
                    { "Services:LoggingService", builder.Configuration["Services:LoggingService"] ?? "local" }
                });

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
                
                //Added Middleware
                
                app.UseRouting();

                app.UseAuthorization();

                // Global error handling
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError(exception, "Unhandled exception occurred");

                        await Task.CompletedTask;
                    });
                });
                
                // Register routes (migrated from RouteConfig)
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Add any area registrations if needed
                app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                
                app.Run();
            }
        }
        
        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }
        }
    }