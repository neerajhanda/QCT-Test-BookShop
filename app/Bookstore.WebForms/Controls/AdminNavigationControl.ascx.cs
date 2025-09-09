using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bookstore.WebForms.Controls
{
    /// <summary>
    /// Admin navigation control that provides breadcrumb navigation and quick actions for admin pages
    /// </summary>
    public partial class AdminNavigationControl : UserControl
    {
        /// <summary>
        /// Gets or sets the current page title for breadcrumb navigation
        /// </summary>
        public string CurrentPageTitle { get; set; }

        /// <summary>
        /// Gets or sets the current page URL for breadcrumb navigation
        /// </summary>
        public string CurrentPageUrl { get; set; }

        /// <summary>
        /// Gets or sets whether to show the quick actions bar
        /// </summary>
        public bool ShowQuickActions { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show the status bar
        /// </summary>
        public bool ShowStatusBar { get; set; } = true;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitializeNavigation();
                SetupBreadcrumb();
                UpdateStatusBar();
            }
        }

        /// <summary>
        /// Initializes the navigation control
        /// </summary>
        private void InitializeNavigation()
        {
            // Set visibility based on properties
            if (!ShowQuickActions)
            {
                var quickActionsDiv = FindControl("admin-quick-actions") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (quickActionsDiv != null)
                {
                    quickActionsDiv.Visible = false;
                }
            }

            // Update admin user display
            if (Page.User != null && Page.User.Identity != null && Page.User.Identity.IsAuthenticated)
            {
                AdminUserLiteral.Text = Page.User.Identity.Name;
            }
            else
            {
                AdminUserLiteral.Text = "Unknown";
            }
        }

        /// <summary>
        /// Sets up the breadcrumb navigation based on current page
        /// </summary>
        private void SetupBreadcrumb()
        {
            if (!string.IsNullOrEmpty(CurrentPageTitle))
            {
                var breadcrumbItem = new Literal();
                breadcrumbItem.Text = string.Format("<li class=\"breadcrumb-item active\" aria-current=\"page\">{0}</li>", CurrentPageTitle);
                BreadcrumbPlaceHolder.Controls.Add(breadcrumbItem);
            }
            else
            {
                // Auto-detect current page based on URL
                var currentUrl = Page.Request.Url.AbsolutePath.ToLower();
                string pageTitle = GetPageTitleFromUrl(currentUrl);
                
                if (!string.IsNullOrEmpty(pageTitle))
                {
                    var breadcrumbItem = new Literal();
                    breadcrumbItem.Text = string.Format("<li class=\"breadcrumb-item active\" aria-current=\"page\">{0}</li>", pageTitle);
                    BreadcrumbPlaceHolder.Controls.Add(breadcrumbItem);
                }
            }
        }

        /// <summary>
        /// Gets the page title based on the URL
        /// </summary>
        /// <param name="url">The current URL</param>
        /// <returns>The page title</returns>
        private string GetPageTitleFromUrl(string url)
        {
            if (url.Contains("/admin/dashboard.aspx"))
                return "Dashboard";
            else if (url.Contains("/admin/orders.aspx"))
                return "Order Management";
            else if (url.Contains("/admin/inventory.aspx"))
                return "Inventory Management";
            else if (url.Contains("/admin/offers.aspx"))
                return "Offers Management";
            else if (url.Contains("/admin/referencedata.aspx"))
                return "Reference Data";
            else
                return string.Empty;
        }

        /// <summary>
        /// Updates the status bar with current information
        /// </summary>
        private void UpdateStatusBar()
        {
            if (ShowStatusBar)
            {
                LastActivityLiteral.Text = DateTime.Now.ToString("MMM dd, yyyy HH:mm");
            }
        }

        /// <summary>
        /// Handles navigation button clicks
        /// </summary>
        /// <param name="sender">The button that was clicked</param>
        /// <param name="e">Event arguments containing the target URL</param>
        protected void NavigateToPage(object sender, EventArgs e)
        {
            if (sender is LinkButton && !string.IsNullOrEmpty(((LinkButton)sender).CommandArgument))
            {
                var button = (LinkButton)sender;
                {
                    try
                    {
                        Response.Redirect(ResolveUrl(button.CommandArgument));
                    }
                    catch (Exception ex)
                    {
                        // Log error and show message
                        // In a real application, you would use proper logging
                        Page.Session["ErrorMessage"] = "Navigation error occurred. Please try again.";
                    }
                }
            }
        }
        

        /// <summary>
        /// Adds a custom breadcrumb item
        /// </summary>
        /// <param name="title">The title of the breadcrumb item</param>
        /// <param name="url">The URL of the breadcrumb item (optional)</param>
        public void AddBreadcrumbItem(string title, string url = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "Breadcrumb title cannot be null or empty");
            }

            var breadcrumbItem = new Literal();
            
            if (!string.IsNullOrEmpty(url))
            {
                breadcrumbItem.Text = string.Format("<li class=\"breadcrumb-item\"><a href=\"{0}\">{1}</a></li>", ResolveUrl(url), title);
            }
            else
            {
                breadcrumbItem.Text = string.Format("<li class=\"breadcrumb-item active\" aria-current=\"page\">{0}</li>", title);
            }
            
            BreadcrumbPlaceHolder.Controls.Add(breadcrumbItem);
        }

        /// <summary>
        /// Highlights the current page button in the quick actions
        /// </summary>
        /// <param name="pageName">The name of the current page</param>
        public void HighlightCurrentPage(string pageName)
        {
            // Reset all buttons to outline style
            DashboardButton.CssClass = "btn btn-outline-primary btn-sm";
            OrdersButton.CssClass = "btn btn-outline-primary btn-sm";
            InventoryButton.CssClass = "btn btn-outline-primary btn-sm";
            OffersButton.CssClass = "btn btn-outline-primary btn-sm";
            ReferenceDataButton.CssClass = "btn btn-outline-primary btn-sm";

            // Highlight the current page button
            switch (pageName.ToLower())
            {
                case "dashboard":
                    DashboardButton.CssClass = "btn btn-primary btn-sm";
                    break;
                case "orders":
                    OrdersButton.CssClass = "btn btn-primary btn-sm";
                    break;
                case "inventory":
                    InventoryButton.CssClass = "btn btn-primary btn-sm";
                    break;
                case "offers":
                    OffersButton.CssClass = "btn btn-primary btn-sm";
                    break;
                case "referencedata":
                    ReferenceDataButton.CssClass = "btn btn-primary btn-sm";
                    break;
            }
        }
    }
}