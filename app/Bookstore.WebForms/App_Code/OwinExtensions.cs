using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Bookstore.WebForms
{
    /// <summary>
    /// Extension methods for OWIN integration with WebForms
    /// </summary>
    public static class OwinExtensions
    {
        /// <summary>
        /// Gets the shopping cart correlation ID from HttpContext
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <returns>The correlation ID</returns>
        public static string GetShoppingCartCorrelationId(this HttpContext context)
        {
            return StateManagement.SessionManager.ShoppingCartCorrelationId;
        }

        /// <summary>
        /// Gets the shopping cart correlation ID from HttpContextBase
        /// </summary>
        /// <param name="context">The HttpContextBase</param>
        /// <returns>The correlation ID</returns>
        public static string GetShoppingCartCorrelationId(this HttpContextBase context)
        {
            return StateManagement.SessionManager.ShoppingCartCorrelationId;
        }
    }

    /// <summary>
    /// Default authentication types for the application
    /// </summary>
    public static class DefaultAuthenticationTypes
    {
        public const string ApplicationCookie = "ApplicationCookie";
        public const string ExternalCookie = "ExternalCookie";
        public const string TwoFactorCookie = "TwoFactorCookie";
        public const string TwoFactorRememberBrowserCookie = "TwoFactorRememberBrowser";
    }
}