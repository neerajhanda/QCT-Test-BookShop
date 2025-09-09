using System;
using System.Web;
using System.Web.Routing;
using NLog;
using Bookstore.WebForms.StateManagement;

namespace Bookstore.WebForms
{
    public class Global : HttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start(object sender, EventArgs e)
        {
            try
            {
                Logger.Info("WebForms application starting up");

                // Register routes for WebForms (if needed for friendly URLs)
                RegisterRoutes(RouteTable.Routes);

                // Run dependency injection integration tests
                var testsPass = DependencyInjectionIntegrationTest.RunTests();
                if (testsPass)
                {
                    Logger.Info("Dependency injection integration tests passed during application startup");
                }
                else
                {
                    Logger.Error("Dependency injection integration tests failed during application startup");
                }

                Logger.Info("WebForms application startup completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during WebForms application startup");
                throw;
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug("New session started: {SessionId}", Session.SessionID);
                
                // Initialize session variables
                SessionManager.UserPreferences = new UserPreferences();
                
                // Log session start for analytics
                Logger.Info("Session started - SessionId: {SessionId}, Timeout: {Timeout} minutes", 
                    Session.SessionID, Session.Timeout);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during session start");
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Handle any pre-request processing
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // Handle authentication-related processing
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // DISABLED FOR DEBUGGING - Let exceptions bubble up to see actual error
            // Exception exception = Server.GetLastError();
            // ... error handling code disabled
        }

        /// <summary>
        /// Handles specific types of errors with appropriate responses
        /// </summary>
        private void HandleSpecificErrors(Exception exception, HttpContext context)
        {
            try
            {
                // Handle HTTP exceptions specifically
                if (exception is HttpException)
                {
                    var httpException = (HttpException)exception;
                    var statusCode = httpException.GetHttpCode();
                    
                    switch (statusCode)
                    {
                        case 404:
                            Logger.Warn("404 Not Found: {RequestUrl}", context?.Request?.Url?.ToString());
                            context?.Response?.Redirect("~/NotFound.aspx");
                            return;
                        case 403:
                            Logger.Warn("403 Forbidden: {RequestUrl}", context?.Request?.Url?.ToString());
                            context?.Response?.Redirect("~/Unauthorized.aspx");
                            return;
                        case 500:
                            Logger.Error("500 Internal Server Error: {RequestUrl}", context?.Request?.Url?.ToString());
                            break;
                    }
                }

                // Handle security-related exceptions
                if (exception is UnauthorizedAccessException || 
                    exception is System.Security.SecurityException)
                {
                    Logger.Warn(exception, "Security exception occurred");
                    context?.Response?.Redirect("~/Unauthorized.aspx");
                    return;
                }

                // Handle database-related exceptions
                if (exception.GetType().Name.Contains("Sql") || 
                    exception.GetType().Name.Contains("Database") ||
                    exception.GetType().Name.Contains("Entity"))
                {
                    Logger.Error(exception, "Database-related error occurred");
                    // Could redirect to a specific database error page if needed
                }

                // Handle AWS service exceptions
                if (exception.GetType().Namespace?.StartsWith("Amazon") == true)
                {
                    Logger.Error(exception, "AWS service error occurred");
                    // Could implement specific AWS error handling
                }
            }
            catch (Exception handlingException)
            {
                Logger.Error(handlingException, "Error occurred while handling specific error types");
            }
        }

        /// <summary>
        /// Redirects to the appropriate error page based on the exception type
        /// </summary>
        private void RedirectToErrorPage(Exception exception)
        {
            try
            {
                var context = HttpContext.Current;
                if (context?.Response != null && !context.Response.IsRequestBeingRedirected)
                {
                    // Default to generic error page
                    context.Response.Redirect("~/Error.aspx");
                }
            }
            catch (Exception redirectException)
            {
                Logger.Error(redirectException, "Failed to redirect to error page");
                // Last resort - try to write a simple error message
                try
                {
                    var response = HttpContext.Current?.Response;
                    if (response != null)
                    {
                        response.Clear();
                        response.StatusCode = 500;
                        response.Write("<html><body><h1>An error occurred</h1><p>Please try again later.</p></body></html>");
                        response.End();
                    }
                }
                catch
                {
                    // If even this fails, there's nothing more we can do
                }
            }
        }

        /// <summary>
        /// Gets the client IP address, considering proxies and load balancers
        /// </summary>
        private string GetClientIPAddress(HttpRequest request)
        {
            try
            {
                if (request == null) return "Unknown";

                var ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = request.ServerVariables["HTTP_X_REAL_IP"];
                
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = request.ServerVariables["REMOTE_ADDR"];
                
                if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(","))
                {
                    // Take the first IP if there are multiple (proxy chain)
                    ipAddress = ipAddress.Split(',')[0].Trim();
                }
                
                return ipAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the current user ID if available
        /// </summary>
        private string GetCurrentUserId()
        {
            try
            {
                return AuthorizationHelper.GetUserId() ?? "Anonymous";
            }
            catch
            {
                return "Unknown";
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug("Session ended: {SessionId}", Session?.SessionID ?? "Unknown");
                
                // Clean up session resources
                // Note: Session data is automatically cleaned up by ASP.NET
                // This is just for logging and any custom cleanup
                
                Logger.Info("Session ended - SessionId: {SessionId}", Session?.SessionID ?? "Unknown");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during session end");
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
            // Clean up application resources
        }

        /// <summary>
        /// Register routes for WebForms application
        /// </summary>
        /// <param name="routes">The route collection</param>
        private void RegisterRoutes(RouteCollection routes)
        {
            // Add any custom routes here if needed for friendly URLs
            // For now, we'll use standard WebForms page routing
        }
    }
}