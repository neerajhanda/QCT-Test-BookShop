using System;
using System.Web;
using NLog;

namespace Bookstore.WebForms
{
    public partial class NotFound : BasePage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    InitializeNotFoundPage();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while loading 404 page");
                // Fallback - show basic message
                RequestedUrlLiteral.Text = "<p>An error occurred while displaying the 404 page.</p>";
            }
        }

        private void InitializeNotFoundPage()
        {
            // Set the HTTP status code to 404
            Response.StatusCode = 404;
            Response.StatusDescription = "Not Found";

            // Get the requested URL
            var requestedUrl = Request.Url?.ToString() ?? "Unknown URL";
            var referrer = Request.UrlReferrer?.ToString();

            // Display the requested URL to the user
            RequestedUrlLiteral.Text = $"<p class=\"requested-url\">Requested URL: <strong>{Server.HtmlEncode(requestedUrl)}</strong></p>";

            // Log the 404 error with additional context
            Logger.Warn("404 Not Found: {RequestedUrl}, Referrer: {Referrer}, User Agent: {UserAgent}, IP: {ClientIP}", 
                requestedUrl, 
                referrer ?? "Direct access", 
                Request.UserAgent ?? "Unknown", 
                GetClientIPAddress());

            // Track common 404 patterns for analysis
            TrackNotFoundPattern(requestedUrl);
        }

        private void TrackNotFoundPattern(string requestedUrl)
        {
            try
            {
                // Log patterns that might indicate issues
                if (!string.IsNullOrEmpty(requestedUrl))
                {
                    var lowerUrl = requestedUrl.ToLowerInvariant();
                    
                    if (lowerUrl.Contains(".php") || lowerUrl.Contains(".jsp"))
                    {
                        Logger.Info("404 - Possible attack or migration issue detected: {RequestedUrl}", requestedUrl);
                    }
                    else if (lowerUrl.Contains("admin") || lowerUrl.Contains("wp-admin"))
                    {
                        Logger.Info("404 - Admin area access attempt: {RequestedUrl}", requestedUrl);
                    }
                    else if (lowerUrl.Contains("api/") || lowerUrl.Contains("/api"))
                    {
                        Logger.Info("404 - API endpoint not found: {RequestedUrl}", requestedUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while tracking 404 pattern");
            }
        }

        private string GetClientIPAddress()
        {
            try
            {
                // Try to get the real IP address, considering proxies and load balancers
                var ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = Request.ServerVariables["HTTP_X_REAL_IP"];
                
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                
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

        protected override void OnError(EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                Logger.Error(exception, "Error occurred on NotFound page");
                Server.ClearError();
                // Redirect to generic error page to avoid infinite loop
                Response.Redirect("~/Error.aspx");
            }
        }
    }
}