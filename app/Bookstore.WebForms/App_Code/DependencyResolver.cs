using System;

namespace Bookstore.WebForms
{
    /// <summary>
    /// Static dependency resolver for WebForms application
    /// </summary>
    public static class DependencyResolver
    {
        private static IWebFormsDependencyResolver _resolver;

        /// <summary>
        /// Gets the current dependency resolver instance
        /// </summary>
        public static IWebFormsDependencyResolver Current
        {
            get
            {
                if (_resolver == null)
                {
                    throw new InvalidOperationException("Dependency resolver has not been initialized. Call SetResolver() during application startup.");
                }
                return _resolver;
            }
        }

        /// <summary>
        /// Sets the dependency resolver instance
        /// </summary>
        /// <param name="resolver">The dependency resolver to use</param>
        public static void SetResolver(IWebFormsDependencyResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException("resolver");
            _resolver = resolver;
        }

        /// <summary>
        /// Resolves a service of the specified type
        /// </summary>
        /// <typeparam name="T">The type of service to resolve</typeparam>
        /// <returns>The resolved service instance</returns>
        public static T Resolve<T>() where T : class
        {
            return Current.Resolve<T>();
        }

        /// <summary>
        /// Resolves a service of the specified type
        /// </summary>
        /// <param name="serviceType">The type of service to resolve</param>
        /// <returns>The resolved service instance</returns>
        public static object Resolve(Type serviceType)
        {
            return Current.Resolve(serviceType);
        }

        /// <summary>
        /// Tries to resolve a service of the specified type
        /// </summary>
        /// <typeparam name="T">The type of service to resolve</typeparam>
        /// <param name="service">The resolved service instance, or default(T) if not found</param>
        /// <returns>True if the service was resolved successfully, false otherwise</returns>
        public static bool TryResolve<T>(out T service) where T : class
        {
            return Current.TryResolve(out service);
        }

        /// <summary>
        /// Tries to resolve a service of the specified type
        /// </summary>
        /// <param name="serviceType">The type of service to resolve</param>
        /// <param name="service">The resolved service instance, or null if not found</param>
        /// <returns>True if the service was resolved successfully, false otherwise</returns>
        public static bool TryResolve(Type serviceType, out object service)
        {
            return Current.TryResolve(serviceType, out service);
        }

        /// <summary>
        /// Injects properties into the specified instance
        /// </summary>
        /// <param name="instance">The instance to inject properties into</param>
        public static void InjectProperties(object instance)
        {
            Current.InjectProperties(instance);
        }
    }
}