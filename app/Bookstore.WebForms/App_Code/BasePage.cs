using System;
using System.Web;
using System.Web.UI;
using Autofac;
using Autofac.Integration.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Host.SystemWeb;
using NLog;
using Bookstore.WebForms.StateManagement;
using Bookstore.WebForms.Controls;

namespace Bookstore.WebForms
{
    /// <summary>
    /// Base page class that provides dependency injection support for WebForms pages
    /// </summary>
    public class BasePage : Page
    {
        private ILifetimeScope _lifetimeScope;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the current Autofac lifetime scope for dependency resolution
        /// </summary>
        protected ILifetimeScope LifetimeScope
        {
            get
            {
                if (_lifetimeScope == null)
                {
                    try
                    {
                        var owinContext = Context.GetOwinContext();
                        _lifetimeScope = owinContext.GetAutofacLifetimeScope();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "OWIN context not available - dependency injection disabled");
                        // Return null when OWIN is not configured - this allows pages to work without DI
                        return null;
                    }
                }
                return _lifetimeScope;
            }
        }

        /// <summary>
        /// Resolves a service of type T using dependency injection
        /// </summary>
        /// <typeparam name="T">The type of service to resolve</typeparam>
        /// <returns>The resolved service instance</returns>
        protected T Resolve<T>()
        {
            try
            {
                var scope = LifetimeScope;
                if (scope == null)
                {
                    Logger.Warn("Dependency injection not available - cannot resolve service of type {ServiceType}", typeof(T).Name);
                    return default(T);
                }
                return scope.Resolve<T>();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to resolve service of type {ServiceType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Tries to resolve a service of type T using dependency injection
        /// </summary>
        /// <typeparam name="T">The type of service to resolve</typeparam>
        /// <param name="service">The resolved service instance, or default(T) if not found</param>
        /// <returns>True if the service was resolved successfully, false otherwise</returns>
        protected bool TryResolve<T>(out T service) where T : class
        {
            try
            {
                var scope = LifetimeScope;
                if (scope == null)
                {
                    service = default(T);
                    return false;
                }
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
        /// Override PreInit to perform dependency injection setup and state management optimization
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            
            try
            {
                // Perform property injection on the current page instance
                var scope = LifetimeScope;
                if (scope != null)
                {
                    scope.InjectProperties(this);
                    Logger.Debug("Property injection completed for page {PageType}", this.GetType().Name);
                }
                else
                {
                    Logger.Debug("Dependency injection not available for page {PageType}", this.GetType().Name);
                }

                // Optimize ViewState for the page
                OptimizePageViewState();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to perform property injection or ViewState optimization for page {PageType}", this.GetType().Name);
                // Don't throw - allow page to continue without DI
                Logger.Warn("Continuing page load without dependency injection");
            }
        }

        /// <summary>
        /// Override PreRender to perform final ViewState optimizations
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            
            try
            {
                // Apply ViewState optimizations
                ViewStateManager.OptimizePageViewState(this);
                
                // Log ViewState information in debug mode
                if (Logger.IsDebugEnabled)
                {
                    ViewStateManager.LogViewStateInfo(this);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during PreRender ViewState optimization for page {PageType}", this.GetType().Name);
            }
        }

        /// <summary>
        /// Virtual method for page-specific ViewState optimization
        /// </summary>
        protected virtual void OptimizePageViewState()
        {
            // Override in derived pages for specific optimizations
        }

        /// <summary>
        /// Override Dispose to properly clean up the lifetime scope
        /// </summary>
        public override void Dispose()
        {
            if (_lifetimeScope != null)
            {
                _lifetimeScope.Dispose();
                _lifetimeScope = null;
            }
            base.Dispose();
        }

        /// <summary>
        /// Helper method to check if the current user is authenticated
        /// </summary>
        protected bool IsUserAuthenticated
        {
            get { return AuthorizationHelper.IsAuthenticated(); }
        }

        /// <summary>
        /// Helper method to get the current user's identity name
        /// </summary>
        protected string CurrentUserName
        {
            get { return AuthorizationHelper.GetUserDisplayName() ?? string.Empty; }
        }

        /// <summary>
        /// Helper method to get the current user's ID
        /// </summary>
        protected string CurrentUserId
        {
            get { return AuthorizationHelper.GetUserId(); }
        }

        /// <summary>
        /// Helper method to check if the current user is in a specific role
        /// </summary>
        /// <param name="role">The role to check</param>
        /// <returns>True if the user is in the role, false otherwise</returns>
        protected bool IsUserInRole(string role)
        {
            return AuthorizationHelper.IsInRole(role);
        }

        /// <summary>
        /// Helper method to check if the current user is an administrator
        /// </summary>
        protected bool IsUserAdministrator
        {
            get { return AuthorizationHelper.IsAdministrator(); }
        }

        /// <summary>
        /// Helper method to redirect to login page if user is not authenticated
        /// </summary>
        protected void RequireAuthentication()
        {
            AuthorizationHelper.RequireAuthentication(Request.RawUrl);
        }

        /// <summary>
        /// Helper method to redirect to login page if user is not in the specified role
        /// </summary>
        /// <param name="role">The required role</param>
        protected void RequireRole(string role)
        {
            AuthorizationHelper.RequireRole(role, Request.RawUrl);
        }

        /// <summary>
        /// Helper method to redirect to login page if user is not an administrator
        /// </summary>
        protected void RequireAdministrator()
        {
            AuthorizationHelper.RequireAdministrator(Request.RawUrl);
        }

        /// <summary>
        /// Helper method to show error messages to the user
        /// </summary>
        /// <param name="message">The error message to display</param>
        protected void ShowError(string message)
        {
            SessionManager.ErrorMessage = message;
        }

        /// <summary>
        /// Helper method to show success messages to the user
        /// </summary>
        /// <param name="message">The success message to display</param>
        protected void ShowSuccess(string message)
        {
            SessionManager.SuccessMessage = message;
        }

        /// <summary>
        /// Helper method to show notification messages to the user
        /// </summary>
        /// <param name="message">The notification message to display</param>
        protected void ShowNotification(string message)
        {
            SessionManager.Notification = message;
        }

        /// <summary>
        /// Gets the shopping cart correlation ID for the current user
        /// </summary>
        protected string GetShoppingCartCorrelationId()
        {
            return SessionManager.ShoppingCartCorrelationId;
        }

        /// <summary>
        /// Gets or sets user preferences
        /// </summary>
        protected UserPreferences UserPreferences
        {
            get { return SessionManager.UserPreferences; }
            set { SessionManager.UserPreferences = value; }
        }

        /// <summary>
        /// Gets or sets search criteria
        /// </summary>
        protected StateManagement.SearchCriteria SearchCriteria
        {
            get { return SessionManager.SearchCriteria; }
            set { SessionManager.SearchCriteria = value; }
        }

        /// <summary>
        /// Override OnError to provide consistent page-level error handling
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnError(EventArgs e)
        {
            // DISABLED FOR DEBUGGING - Let exceptions bubble up to see actual error
            // base.OnError(e);
        }

        /// <summary>
        /// Safe method to execute code with error handling and logging
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="actionName">Name of the action for logging purposes</param>
        /// <param name="showErrorToUser">Whether to show error message to user</param>
        /// <returns>True if successful, false if an error occurred</returns>
        protected bool ExecuteSafely(Action action, string actionName, bool showErrorToUser = true)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing {ActionName} in {PageType}", actionName, this.GetType().Name);
                
                if (showErrorToUser)
                {
                    ShowError(string.Format("An error occurred while {0}. Please try again.", actionName.ToLower()));
                }
                
                return false;
            }
        }

        /// <summary>
        /// Safe method to execute code with error handling, logging, and return value
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="func">The function to execute</param>
        /// <param name="actionName">Name of the action for logging purposes</param>
        /// <param name="defaultValue">Default value to return on error</param>
        /// <param name="showErrorToUser">Whether to show error message to user</param>
        /// <returns>Result of function or default value on error</returns>
        protected T ExecuteSafely<T>(Func<T> func, string actionName, T defaultValue = default(T), bool showErrorToUser = true)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing {ActionName} in {PageType}", actionName, this.GetType().Name);
                
                if (showErrorToUser)
                {
                    ShowError(string.Format("An error occurred while {0}. Please try again.", actionName.ToLower()));
                }
                
                return defaultValue;
            }
        }

        /// <summary>
        /// Logs user actions for audit and debugging purposes
        /// </summary>
        /// <param name="action">Description of the action</param>
        /// <param name="additionalData">Additional data to log</param>
        protected void LogUserAction(string action, object additionalData = null)
        {
            try
            {
                if (additionalData != null)
                {
                    Logger.Info("User action: {Action} - User: {UserId}, Page: {PageType}, Data: {@AdditionalData}", 
                        action, CurrentUserId ?? "Anonymous", this.GetType().Name, additionalData);
                }
                else
                {
                    Logger.Info("User action: {Action} - User: {UserId}, Page: {PageType}", 
                        action, CurrentUserId ?? "Anonymous", this.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to log user action: {Action}", action);
            }
        }

        /// <summary>
        /// Validates that required services are available
        /// </summary>
        /// <param name="services">Services to validate</param>
        /// <returns>True if all services are available</returns>
        protected bool ValidateServices(params object[] services)
        {
            try
            {
                foreach (var service in services)
                {
                    if (service == null)
                    {
                        Logger.Error("Required service is null in {PageType}", this.GetType().Name);
                        ShowError("A required service is not available. Please try again later.");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error validating services in {PageType}", this.GetType().Name);
                ShowError("An error occurred while validating services. Please try again later.");
                return false;
            }
        }
    }
}