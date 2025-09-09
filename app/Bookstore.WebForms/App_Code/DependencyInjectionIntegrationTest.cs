using System;
using System.Web;
using Autofac;
using Bookstore.Domain.Books;
using NLog;

namespace Bookstore.WebForms
{
    /// <summary>
    /// Simple integration test to verify dependency injection is working correctly
    /// This class can be used to test DI functionality during development
    /// </summary>
    public static class DependencyInjectionIntegrationTest
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Tests the dependency injection setup by attempting to resolve services
        /// </summary>
        /// <returns>True if all tests pass, false otherwise</returns>
        public static bool RunTests()
        {
            try
            {
                Logger.Info("Starting dependency injection integration tests");

                // Test 1: Verify static resolver is initialized
                if (!TestStaticResolverInitialization())
                {
                    Logger.Error("Static resolver initialization test failed");
                    return false;
                }

                // Test 2: Test service resolution (this will only work in HTTP context)
                if (HttpContext.Current != null)
                {
                    if (!TestServiceResolution())
                    {
                        Logger.Error("Service resolution test failed");
                        return false;
                    }
                }
                else
                {
                    Logger.Info("Skipping service resolution test - no HTTP context available");
                }

                Logger.Info("All dependency injection integration tests passed");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Dependency injection integration tests failed with exception");
                return false;
            }
        }

        private static bool TestStaticResolverInitialization()
        {
            try
            {
                // This should not throw an exception if the resolver is properly initialized
                var resolver = DependencyResolver.Current;
                Logger.Info("Static resolver initialization test passed");
                return true;
            }
            catch (InvalidOperationException)
            {
                Logger.Error("Static resolver is not initialized");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unexpected error during static resolver initialization test");
                return false;
            }
        }

        private static bool TestServiceResolution()
        {
            try
            {
                // Test resolving a service that should be registered
                var bookService = DependencyResolver.Resolve<IBookService>();
                if (bookService == null)
                {
                    Logger.Error("Book service resolved to null");
                    return false;
                }

                Logger.Info("Service resolution test passed - IBookService resolved successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Service resolution test failed");
                return false;
            }
        }

        /// <summary>
        /// Tests property injection on a test object
        /// </summary>
        /// <returns>True if property injection works, false otherwise</returns>
        public static bool TestPropertyInjection()
        {
            if (HttpContext.Current == null)
            {
                Logger.Info("Skipping property injection test - no HTTP context available");
                return true;
            }

            try
            {
                var testObject = new TestObjectWithDependencies();
                DependencyResolver.InjectProperties(testObject);

                if (testObject.BookService == null)
                {
                    Logger.Error("Property injection failed - BookService is null");
                    return false;
                }

                Logger.Info("Property injection test passed");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Property injection test failed");
                return false;
            }
        }

        /// <summary>
        /// Test class for property injection testing
        /// </summary>
        public class TestObjectWithDependencies
        {
            public IBookService BookService { get; set; }
        }
    }
}