using System;
using System.Web;
using System.Web.UI;
using Microsoft.Owin.Security;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Extensions;
using Microsoft.AspNet.Identity;

namespace Bookstore.WebForms.Admin
{
    public partial class AdminMaster : MasterPage
    {
        /// <summary>
        /// Gets the admin navigation control for use by content pages
        /// </summary>
        public Controls.AdminNavigationControl AdminNavigationControl
        {
            get { return AdminNavigation; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user is authenticated and has admin role
                CheckAdminAccess();
                SetupAdminNavigation();
                DisplayMessages();
                ConfigureNavigationControl();
            }
        }

        /// <summary>
        /// Checks if the current user has admin access
        /// </summary>
        private void CheckAdminAccess()
        {
            // Check if user is authenticated
            if (Page.User == null || !Page.User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.ToString()));
                return;
            }

            // Check if user has admin role
            if (!Page.User.IsInRole("Administrators"))
            {
                // Redirect to unauthorized page or main site
                Session["ErrorMessage"] = "You do not have permission to access the admin area.";
                Response.Redirect("~/Default.aspx");
                return;
            }
        }

        /// <summary>
        /// Sets up admin navigation based on user authentication status
        /// </summary>
        private void SetupAdminNavigation()
        {
            if (Page.User != null && Page.User.Identity != null && Page.User.Identity.IsAuthenticated)
            {
                AdminUserNameLiteral.Text = Page.User.Identity.Name;
                AdminWelcome.Visible = true;
            }
            else
            {
                AdminWelcome.Visible = false;
            }
        }

        /// <summary>
        /// Displays error or success messages from session
        /// </summary>
        private void DisplayMessages()
        {
            // Display error message if present
            if (Session["ErrorMessage"] != null)
            {
                ErrorMessage.Text = Session["ErrorMessage"].ToString();
                ErrorPanel.Visible = true;
                Session.Remove("ErrorMessage");
            }

            // Display success message if present
            if (Session["SuccessMessage"] != null)
            {
                SuccessMessage.Text = Session["SuccessMessage"].ToString();
                SuccessPanel.Visible = true;
                Session.Remove("SuccessMessage");
            }
        }

        /// <summary>
        /// Configures the admin navigation control
        /// </summary>
        private void ConfigureNavigationControl()
        {
            if (AdminNavigation != null)
            {
                // Set current page title based on the current page
                var currentUrl = Request.Url.AbsolutePath.ToLower();
                string currentPageName = GetCurrentPageName(currentUrl);
                
                if (!string.IsNullOrEmpty(currentPageName))
                {
                    AdminNavigation.HighlightCurrentPage(currentPageName);
                }
            }
        }

        /// <summary>
        /// Gets the current page name from the URL
        /// </summary>
        /// <param name="url">The current URL</param>
        /// <returns>The page name</returns>
        private string GetCurrentPageName(string url)
        {
            if (url.Contains("/admin/dashboard.aspx"))
                return "dashboard";
            else if (url.Contains("/admin/orders.aspx"))
                return "orders";
            else if (url.Contains("/admin/inventory.aspx"))
                return "inventory";
            else if (url.Contains("/admin/offers.aspx"))
                return "offers";
            else if (url.Contains("/admin/referencedata.aspx"))
                return "referencedata";
            else
                return string.Empty;
        }

        /// <summary>
        /// Handles the admin logout button click event
        /// </summary>
        /// <param name="sender">The logout button</param>
        /// <param name="e">Event arguments</param>
        protected void AdminLogoutButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Sign out using OWIN authentication
                var authenticationManager = Context.GetOwinContext().Authentication;
                authenticationManager.SignOut("ApplicationCookie");

                // Clear session
                Session.Clear();
                Session.Abandon();

                // Redirect to home page
                Response.Redirect("~/Default.aspx");
            }
            catch (Exception ex)
            {
                // Log error and show message to user
                Session["ErrorMessage"] = "An error occurred during logout. Please try again.";
                // In a real application, you would log this error
                // Logger.LogError(ex, "Error during admin logout");
            }
        }
    }
}