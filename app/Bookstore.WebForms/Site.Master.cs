using System;
using System.Web.UI;
using Microsoft.Owin.Security;
using Bookstore.WebForms.StateManagement;

namespace Bookstore.WebForms
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetupNavigation();
                DisplayMessages();
            }
        }

        /// <summary>
        /// Sets up navigation based on user authentication status
        /// </summary>
        private void SetupNavigation()
        {
            bool isAuthenticated = AuthorizationHelper.IsAuthenticated();

            // Show/hide navigation elements based on authentication
            AuthenticatedNavigation.Visible = isAuthenticated;
            AuthenticatedNavigation2.Visible = isAuthenticated;
            AuthenticatedWelcome.Visible = isAuthenticated;
            AnonymousNavigation.Visible = !isAuthenticated;

            if (isAuthenticated)
            {
                UserNameLiteral.Text = AuthorizationHelper.GetUserDisplayName();
                
                // Show admin portal link if user is in admin role
                AdminPortalLink.Visible = AuthorizationHelper.IsAdministrator();
            }
        }

        /// <summary>
        /// Displays error or success messages from session
        /// </summary>
        private void DisplayMessages()
        {
            // Display error message if present
            var errorMessage = SessionManager.ErrorMessage;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorMessage.Text = errorMessage;
                ErrorPanel.Visible = true;
                SessionManager.ClearErrorMessage();
            }

            // Display success message if present
            var successMessage = SessionManager.SuccessMessage;
            if (!string.IsNullOrEmpty(successMessage))
            {
                SuccessMessage.Text = successMessage;
                SuccessPanel.Visible = true;
                SessionManager.ClearSuccessMessage();
            }
        }

        /// <summary>
        /// Handles the logout button click event
        /// </summary>
        /// <param name="sender">The logout button</param>
        /// <param name="e">Event arguments</param>
        protected void LogoutButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Redirect to logout URL which will be handled by the authentication system
                Response.Redirect(AuthorizationHelper.GetLogoutUrl());
            }
            catch (Exception ex)
            {
                // Log error and show message to user
                SessionManager.ErrorMessage = "An error occurred during logout. Please try again.";
                // In a real application, you would log this error
                System.Diagnostics.Debug.WriteLine($"Error during logout: {ex.Message}");
            }
        }
    }
}