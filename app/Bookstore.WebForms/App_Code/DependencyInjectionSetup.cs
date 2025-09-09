using System;
using System.IO;
using System.Reflection;
using System.Web;
using Amazon.Rekognition;
using Amazon.S3;
using Autofac;
using Autofac.Integration.Owin;
using BobsBookstoreClassic.Data;
using Bookstore.Data;
using Bookstore.Data.FileServices;
using Bookstore.Data.ImageResizeService;
using Bookstore.Data.ImageValidationServices;
using Bookstore.Data.Repositories;
using Bookstore.Domain;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;
using Owin;

namespace Bookstore.WebForms
{
    /// <summary>
    /// Configures dependency injection for the WebForms application
    /// </summary>
    public static class DependencyInjectionSetup
    {
        /// <summary>
        /// Configures the Autofac container for WebForms dependency injection
        /// </summary>
        /// <param name="app">The OWIN app builder</param>
        public static void ConfigureDependencyInjection(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            // Register domain services
            builder.RegisterType<BookService>().As<IBookService>();
            builder.RegisterType<OrderService>().As<IOrderService>();
            builder.RegisterType<ReferenceDataService>().As<IReferenceDataService>();
            builder.RegisterType<OfferService>().As<IOfferService>();
            builder.RegisterType<CustomerService>().As<ICustomerService>();
            builder.RegisterType<AddressService>().As<IAddressService>();
            builder.RegisterType<ShoppingCartService>().As<IShoppingCartService>();
            builder.RegisterType<ImageResizeService>().As<IImageResizeService>();

            // Register Entity Framework DbContext
            string connectionString;
            try
            {
                connectionString = BookstoreConfiguration.GetConnectionString("BookstoreDatabaseConnection");
            }
            catch (Exception)
            {
                // Fallback to direct config access if BookstoreConfiguration fails
                var connStringSettings = System.Configuration.ConfigurationManager.ConnectionStrings["BookstoreDatabaseConnection"];
                connectionString = connStringSettings != null ? connStringSettings.ConnectionString : null;
            }
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.Register(c => 
                {
                    var context = new ApplicationDbContext(connectionString);
                    // Set a reasonable timeout to prevent hanging
                    context.Database.CommandTimeout = 30;
                    return context;
                }).InstancePerRequest();
            }

            // Register repositories
            builder.RegisterType<CustomerRepository>().As<ICustomerRepository>();
            builder.RegisterType<AddressRepository>().As<IAddressRepository>();
            builder.RegisterType<BookRepository>().As<IBookRepository>();
            builder.RegisterType<OfferRepository>().As<IOfferRepository>();
            builder.RegisterType<ShoppingCartRepository>().As<IShoppingCartRepository>();
            builder.RegisterType<OrderRepository>().As<IOrderRepository>();
            builder.RegisterType<ReferenceDataRepository>().As<IReferenceDataRepository>();

            // Register generic types
            builder.RegisterGeneric(typeof(PaginatedList<>)).As(typeof(IPaginatedList<>)).InstancePerLifetimeScope();

            // Configure file service based on configuration
            string fileServiceSetting;
            try
            {
                fileServiceSetting = BookstoreConfiguration.GetSetting("Services/FileService");
            }
            catch (Exception)
            {
                // Fallback to direct config access
                fileServiceSetting = System.Configuration.ConfigurationManager.AppSettings["Services/FileService"] ?? "local";
            }
            
            if (fileServiceSetting == "aws")
            {
                builder.RegisterType<AmazonS3Client>().As<IAmazonS3>();
                builder.RegisterType<S3FileService>().As<IFileService>();
            }
            else
            {
                var webRootPath = HttpRuntime.AppDomainAppVirtualPath != null ?
                    Path.Combine(HttpRuntime.AppDomainAppPath, "Content") :
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                builder.RegisterInstance(new LocalFileService(webRootPath)).As<IFileService>();
            }

            // Configure image validation service based on configuration
            string imageValidationSetting;
            try
            {
                imageValidationSetting = BookstoreConfiguration.GetSetting("Services/ImageValidationService");
            }
            catch (Exception)
            {
                // Fallback to direct config access
                imageValidationSetting = System.Configuration.ConfigurationManager.AppSettings["Services/ImageValidationService"] ?? "local";
            }
            
            if (imageValidationSetting == "aws")
            {
                builder.RegisterType<AmazonRekognitionClient>().As<IAmazonRekognition>();
                builder.RegisterType<RekognitionImageValidationService>().As<IImageValidationService>();
            }
            else
            {
                builder.RegisterType<LocalImageValidationService>().As<IImageValidationService>();
            }

            // Register the WebForms dependency resolver
            builder.RegisterType<WebFormsDependencyResolver>().As<IWebFormsDependencyResolver>().SingleInstance();

            // Build the container
            var container = builder.Build();

            // Configure OWIN to use Autofac
            app.UseAutofacMiddleware(container);

            // Set the static dependency resolver
            DependencyResolver.SetResolver(new WebFormsDependencyResolver());
        }
    }
}