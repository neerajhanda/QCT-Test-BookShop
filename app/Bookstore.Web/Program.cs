
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
using WebOptimizer;

namespace Bookstore
{
    public class Program
    {
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Set up database configuration - manual registration instead of AddDbContext
        var connectionString = builder.Configuration.GetConnectionString("BookstoreDatabaseConnection");
        builder.Services.AddSingleton<DbContext>(new DbContext()); // Replace with proper initialization

        // Store configuration in static ConfigurationManager
        ConfigurationManager.Configuration = builder.Configuration;

        // Add services to the container (formerly ConfigureServices)
        builder.Services.AddControllersWithViews()
            .AddViewOptions(options =>
            {
                options.HtmlHelperOptions.ClientValidationEnabled =
                    builder.Configuration.GetValue<bool>("ClientValidationEnabled", true);
            });

        // Register area services (replacing AreaRegistration.RegisterAllAreas())
        builder.Services.AddMvc()
            .AddMvcOptions(options => {
                // Configure any MVC options here
            });

        // Add optimization services (replacing BundleConfig)
        builder.Services.AddWebOptimizer(pipeline =>
        {
            // Configure bundling and minification here if needed
        });

        // Configure environment and service settings
        var environment = builder.Configuration.GetValue<string>("Environment", "Development");
        var authService = builder.Configuration.GetValue<string>("Services/Authentication", "local");
        var dbService = builder.Configuration.GetValue<string>("Services/Database", "local");
        var fileService = builder.Configuration.GetValue<string>("Services/FileService", "local");
        var imageValidationService = builder.Configuration.GetValue<string>("Services/ImageValidationService", "local");
        var loggingService = builder.Configuration.GetValue<string>("Services/LoggingService", "local");

        // Configure AWS Cognito authentication if enabled
        if (authService == "aws")
        {
// Add Cognito authentication services here using the config values
            var cognitoLocalClientId = builder.Configuration.GetValue<string>("Authentication/Cognito/LocalClientId");
            var cognitoAppRunnerClientId = builder.Configuration.GetValue<string>("Authentication/Cognito/AppRunnerClientId");
            var cognitoMetadataAddress = builder.Configuration.GetValue<string>("Authentication/Cognito/MetadataAddress");
            var cognitoDomain = builder.Configuration.GetValue<string>("Authentication/Cognito/CognitoDomain");

            // Add AWS authentication services
        }

        // Configure AWS file service if enabled
        if (fileService == "aws")
        {
            var bucketName = builder.Configuration.GetValue<string>("Files/BucketName");
            var cloudFrontDomain = builder.Configuration.GetValue<string>("Files/CloudFrontDomain");

            // Add AWS file services
        }

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

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

            // Configure middleware to handle application errors
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;

                    if (exception != null)
                    {
                        logger.LogError(exception, "An unhandled exception occurred.");
                    }

                    await Task.CompletedTask;
                });
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Use WebOptimizer (replacing BundleConfig)
            app.UseWebOptimizer();

            app.UseRouting();

            app.UseAuthorization();

            // Register routes (replacing RouteConfig)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapAreaControllerRoute(
                name: "areas",
                areaName: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }

    public class ConfigurationManager
    {
        public static IConfiguration Configuration { get; set; }
    }

    // Placeholder for EntityFramework DbContext
    // This will be replaced with your actual DbContext implementation
    public class DbContext
    {
        // Add constructor and connection string handling
        public DbContext() { }

        public DbContext(string connectionString)
        {
            // Initialize with connection string
        }
    }
}