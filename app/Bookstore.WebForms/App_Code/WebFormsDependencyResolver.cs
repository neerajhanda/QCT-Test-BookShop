using System;
using System.Web;
using Autofac;
using Autofac.Integration.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using NLog;

namespace Bookstore.WebForms
{
    /// <summary>
    /// WebForms-specific dependency resolver implementation using Autofac
    /// </summary>
    public class WebFormsDependencyResolver : IWebFormsDependencyResolver
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the current Autofac lifetime scope from the OWIN context
        /// </summary>
        private ILifetimeScope GetLifetimeScope()
        {
            try
            {
                var httpContext = HttpContext.Current;
                if (httpContext == null)
                {
                    throw new InvalidOperationException("HttpContext.Current is null. This resolver can only be used within an HTTP request context.");
                }

                var owinContext = httpContext.GetOwinContext();
                return owinContext.GetAutofacLifetimeScope();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get Autofac lifetime scope from OWIN context");
                throw new InvalidOperationException("Dependency injection is not properly configured. Ensure OWIN startup is configured correctly.", ex);
            }
        }

        /// <summary>
        /// Resolves a service of the specified type
        /// </summary>
        /// <typeparam name="T">The type of service to resolve</typeparam>
        /// <returns>The resolved service instance</returns>
        public T Resolve<T>() where T : class
        {
            try
            {
                var scope = GetLifetimeScope();
                return scope.Resolve<T>();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to resolve service of type {ServiceType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Resolves a service of the specified type
        /// </summary>
        /// <param name="serviceType">The type of service to resolve</param>
        /// <returns>The resolved service instance</returns>
        public object Resolve(Type serviceType)
        {
            try
            {
                var scope = GetLifetimeScope();
                return scope.Resolve(serviceType);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to resolve service of type {ServiceType}", serviceType.Name);
                throw;
            }
        }

        /// <summary>
        /// Tries to resolve a service of the specified type
        /// </summary>
        /// <typeparam name="T">The type of service to resolve</typeparam>
        /// <param name="service">The resolved service instance, or default(T) if not found</param>
        /// <returns>True if the service was resolved successfully, false otherwise</returns>
        public bool TryResolve<T>(out T service) where T : class
        {
            try
            {
                var scope = GetLifetimeScope();
                return scope.TryResolve(out service);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while trying to resolve service of type {ServiceType}", typeof(T).Name);
                service = default(T);
                return false;
            }
        }

        /// <summary>
        /// Tries to resolve a service of the specified type
        /// </summary>
        /// <param name="serviceType">The type of service to resolve</param>
        /// <param name="service">The resolved service instance, or null if not found</param>
        /// <returns>True if the service was resolved successfully, false otherwise</returns>
        public bool TryResolve(Type serviceType, out object service)
        {
            try
            {
                var scope = GetLifetimeScope();
                return scope.TryResolve(serviceType, out service);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while trying to resolve service of type {ServiceType}", serviceType.Name);
                service = null;
                return false;
            }
        }

        /// <summary>
        /// Injects properties into the specified instance
        /// </summary>
        /// <param name="instance">The instance to inject properties into</param>
        public void InjectProperties(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            try
            {
                var scope = GetLifetimeScope();
                scope.InjectProperties(instance);
                Logger.Debug("Property injection completed for instance of type {InstanceType}", instance.GetType().Name);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to perform property injection for instance of type {InstanceType}", instance.GetType().Name);
                throw;
            }
        }
    }
}