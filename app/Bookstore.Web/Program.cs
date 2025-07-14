
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
using Microsoft.AspNetCore.Routing;
using System.Data.Entity;
using Microsoft.AspNetCore.Http;

    namespace Bookstore
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);
                
                // Add connection strings from Web.config
                builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                // Add connection string from Web.config
// Register EF6 DbContext using a factory pattern
                builder.Services.AddScoped<DbContext>(_ =>
                    new DbContext(builder.Configuration.GetConnectionString("BookstoreDatabaseConnection")
                    ?? "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=BookStoreClassic;MultipleActiveResultSets=true;Integrated Security=SSPI;"));

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;

                // Add services to the container (formerly ConfigureServices)
                builder.Services.AddControllersWithViews(options => {
                    options.EnableEndpointRouting = true;
                });
                builder.Services.AddRazorPages();

                // Register MVC areas
                builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

                // Configure settings from web.config appSettings
                builder.Services.Configure<MvcOptions>(options => {
                    options.EnableEndpointRouting = true;
                });

                // Add logging
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
                builder.Logging.AddDebug();

                // Add application settings from Web.config
                var environmentSetting = builder.Configuration["Environment"] ?? "Development";
                var authService = builder.Configuration["Services/Authentication"] ?? "local";
                var dbService = builder.Configuration["Services/Database"] ?? "local";
                var fileService = builder.Configuration["Services/FileService"] ?? "local";
                var imageValidationService = builder.Configuration["Services/ImageValidationService"] ?? "local";
                var loggingService = builder.Configuration["Services/LoggingService"] ?? "local";


                
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

                // Configure error handling
                app.UseExceptionHandler(errorApp => {
                    errorApp.Run(async context => {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        if (exception != null)
                        {
                            logger.LogError(exception, "An unhandled exception occurred");
                        }

                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
                    });
                });

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.MapAreaControllerRoute(
                    name: "areas",
                    areaName: "{area}",
                    pattern: "{area}/{controller=Home}/{action=Index}/{id?}");

                app.MapRazorPages();

                
                app.Run();
            }
        }
        
        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }
        }
    }