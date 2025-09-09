using System;

namespace Bookstore.WebForms
{
    /// <summary>
    /// Interface for WebForms dependency resolution
    /// </summary>
    public interface IWebFormsDependencyResolver
    {
        /// <summary>
        /// Resolves a service of the specified type
        /// </summary>
        /// <typeparam name="T">The type of service to resolve</typeparam>
        /// <returns>The resolved service instance</returns>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Resolves a service of the specified type
        /// </summary>
        /// <param name="serviceType">The type of service to resolve</param>
        /// <returns>The resolved service instance</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Tries to resolve a service of the specified type
        /// </summary>
        /// <typeparam name="T">The type of service to resolve</typeparam>
        /// <param name="service">The resolved service instance, or default(T) if not found</param>
        /// <returns>True if the service was resolved successfully, false otherwise</returns>
        bool TryResolve<T>(out T service) where T : class;

        /// <summary>
        /// Tries to resolve a service of the specified type
        /// </summary>
        /// <param name="serviceType">The type of service to resolve</param>
        /// <param name="service">The resolved service instance, or null if not found</param>
        /// <returns>True if the service was resolved successfully, false otherwise</returns>
        bool TryResolve(Type serviceType, out object service);

        /// <summary>
        /// Injects properties into the specified instance
        /// </summary>
        /// <param name="instance">The instance to inject properties into</param>
        void InjectProperties(object instance);
    }
}