using System;
using System.Web;
using NLog;

namespace Bookstore.WebForms
{
    public partial class Unauthorized : BasePage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    InitializeUnauthorizedPage();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while loading unauthorized page");
                // Fallback error handling
                UnauthorizedDetailsLiteral.Text = "<p>An error occurred while displaying the access denied page.</p>";
            }
        }

        private void InitializeUnauthorizedPage()
        {
            // Set the HTTP status code to 403
            Response.StatusCode = 403;
            Response.StatusDescription = "Forbidden";

            // Get the requested resource and reason for denial
            var requestedUrl = Request.Url?.ToString() ?? "Unknown URL";
            var returnUrl = Request.QueryString["ReturnUrl"];
            var reason = Request.QueryString["reason"];

            // Display the requested resource to the user
            UnauthorizedDetailsLiteral.Text = $"<p class=\"requested-resource\">Requested resource: <strong>{Server.HtmlEncode(requestedUrl)}</strong></p>";

            // Determine the type of authorization failure and show appropriate guidance
            if (!IsUserAuthenticated)
            {
                // User is not logged in
                LoginPanel.Visible = true;
                
                // Set the return URL for login
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    LoginLink.NavigateUrl = $"~/Login.aspx?ReturnUrl={Server.UrlEncode(returnUrl)}";
                }
                else
                {
                    LoginLink.NavigateUrl = $"~/Login.aspx?ReturnUrl={Server.UrlEncode(Request.RawUrl)}";
                }

                Logger.Warn("Unauthorized access attempt - User not authenticated: {RequestedUrl}, IP: {ClientIP}", 
                    requestedUrl, GetClientIPAddress());
            }
            else
            {
                // User is logged in but doesn't have sufficient privileges
                InsufficientRolePanel.Visible = true;
                LogoutPanel.Visible = true;

                Logger.Warn("Unauthorized access attempt - Insufficient privileges: User: {UserId}, Requested: {RequestedUrl}, IP: {ClientIP}", 
                    CurrentUserId, requestedUrl, GetClientIPAddress());
            }

            // Log additional context if reason is provided
            if (!string.IsNullOrEmpty(reason))
            {
                Logger.Info("Authorization failure reason: {Reason}", reason);
            }
        }

        protected void LogoutButton_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Info("User {UserId} logging out from unauthorized page", CurrentUserId);
                
                // Clear authentication
                AuthorizationHelper.SignOut();
                
                // Redirect to home page
                Response.Redirect("~/");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred during logout from unauthorized page");
                Response.Redirect("~/");
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
                Logger.Error(exception, "Error occurred on Unauthorized page");
                Server.ClearError();
                // Redirect to generic error page to avoid infinite loop
                Response.Redirect("~/Error.aspx");
            }
        }
    }
}