using System;
using System.Web.UI;
using NLog;

namespace Bookstore.WebForms
{
    /// <summary>
    /// Base page class for admin pages that provides admin-specific functionality and authorization
    /// </summary>
    public abstract class AdminBasePage : BasePage
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Override PreInit to perform admin authorization checks before dependency injection
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreInit(EventArgs e)
        {
            // Perform admin authorization check first
            RequireAdministrator();
            
            // Call base PreInit for dependency injection
            base.OnPreInit(e);
            
            Logger.Debug("Admin page {PageType} initialized for user {UserId}", 
                this.GetType().Name, CurrentUserId);
        }

        /// <summary>
        /// Override Load to perform admin-specific initialization
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (!IsPostBack)
            {
                InitializeAdminPage();
            }
        }

        /// <summary>
        /// Virtual method for admin pages to override for page-specific initialization
        /// </summary>
        protected virtual void InitializeAdminPage()
        {
            // Default implementation - can be overridden by derived pages
        }

        /// <summary>
        /// Logs admin actions for audit purposes
        /// </summary>
        /// <param name="action">The action being performed</param>
        /// <param name="details">Optional details about the action</param>
        protected virtual void LogAdminAction(string action, string details = null)
        {
            try
            {
                var logMessage = string.Format("Admin Action: {0}", action);
                if (!string.IsNullOrEmpty(details))
                {
                    logMessage += string.Format(" - Details: {0}", details);
                }
                
                Logger.Info("Admin user {UserId} ({UserName}) performed action: {Action} on page {PageType}. Details: {Details}",
                    CurrentUserId, CurrentUserName, action, this.GetType().Name, details ?? "None");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to log admin action: {Action}", action);
            }
        }

        /// <summary>
        /// Helper method to get the admin user's display name
        /// </summary>
        protected string AdminUserDisplayName
        {
            get
            {
                try
                {
                    return CurrentUserName ?? "Unknown Admin";
                }
                catch
                {
                    return "Unknown Admin";
                }
            }
        }

        /// <summary>
        /// Helper method to check if the current admin user has specific permissions
        /// This can be extended based on more granular admin permissions
        /// </summary>
        /// <param name="permission">The permission to check</param>
        /// <returns>True if the admin has the permission</returns>
        protected virtual bool HasAdminPermission(string permission)
        {
            // For now, all administrators have all permissions
            // This can be extended to support more granular permissions
            return IsUserAdministrator;
        }

        /// <summary>
        /// Helper method to require specific admin permissions
        /// </summary>
        /// <param name="permission">The required permission</param>
        protected void RequireAdminPermission(string permission)
        {
            if (!HasAdminPermission(permission))
            {
                Logger.Warn("Admin user {UserId} attempted to access page {PageType} without permission {Permission}",
                    CurrentUserId, this.GetType().Name, permission);
                
                ShowError("You do not have permission to perform this action.");
                Response.Redirect("~/Admin/Dashboard.aspx");
            }
        }

        /// <summary>
        /// Override ShowError to include admin-specific error logging
        /// </summary>
        /// <param name="message">The error message to display</param>
        protected new void ShowError(string message)
        {
            Logger.Warn("Admin page {PageType} showing error to user {UserId}: {ErrorMessage}",
                this.GetType().Name, CurrentUserId, message);
            
            base.ShowError(message);
        }

        /// <summary>
        /// Override ShowSuccess to include admin-specific success logging
        /// </summary>
        /// <param name="message">The success message to display</param>
        protected new void ShowSuccess(string message)
        {
            Logger.Info("Admin page {PageType} showing success to user {UserId}: {SuccessMessage}",
                this.GetType().Name, CurrentUserId, message);
            
            base.ShowSuccess(message);
        }
    }
}