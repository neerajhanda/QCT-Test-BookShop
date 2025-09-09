using System;
using System.Web;
using System.Web.UI;
using NLog;

namespace Bookstore.WebForms
{
    public partial class Error : BasePage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    InitializeErrorPage();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while loading error page");
                // Fallback error handling - don't redirect to avoid infinite loop
                ErrorDetailsLiteral.Text = "An error occurred while displaying the error page.";
                ErrorDetailsPanel.Visible = true;
            }
        }

        private void InitializeErrorPage()
        {
            // Get error information from session or query string
            var errorMessage = Session["LastErrorMessage"] as string;
            var errorDetails = Session["LastErrorDetails"] as string;
            var showDetails = Request.QueryString["showDetails"] == "true";

            // Clear error information from session
            Session.Remove("LastErrorMessage");
            Session.Remove("LastErrorDetails");

            // Show error details if available and in debug mode or explicitly requested
            if (!string.IsNullOrEmpty(errorDetails) && (HttpContext.Current.IsDebuggingEnabled || showDetails))
            {
                ErrorDetailsLiteral.Text = Server.HtmlEncode(errorDetails);
                ErrorDetailsPanel.Visible = true;
            }

            // Log the error page access
            Logger.Info("Error page accessed. Message: {ErrorMessage}, Details shown: {DetailsShown}", 
                errorMessage ?? "Unknown error", ErrorDetailsPanel.Visible);
        }

        protected void RetryButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the referring URL from session or use home page as fallback
                var returnUrl = Session["ErrorReturnUrl"] as string ?? "~/";
                Session.Remove("ErrorReturnUrl");

                Logger.Info("User clicked retry button, redirecting to: {ReturnUrl}", returnUrl);
                Response.Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while handling retry button click");
                Response.Redirect("~/");
            }
        }

        protected override void OnError(EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                Logger.Error(exception, "Error occurred on Error page itself");
                // Don't call base.OnError to avoid potential infinite loop
                Server.ClearError();
            }
        }
    }
}