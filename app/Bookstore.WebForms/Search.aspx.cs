using System;
using System.Threading.Tasks;
using System.Web.UI;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.WebForms.Controls;
using System.Web;
using Bookstore.WebForms.StateManagement;

namespace Bookstore.WebForms
{
    public partial class Search : BasePage
    {
        public IBookService BookService { get; set; }
        public IShoppingCartService ShoppingCartService { get; set; }

        private string SearchString
        {
            get { return ViewState["SearchString"] as string ?? string.Empty; }
            set { ViewState["SearchString"] = value; }
        }

        private string SortBy
        {
            get { return ViewState["SortBy"] as string ?? "Name"; }
            set { ViewState["SortBy"] = value; }
        }

        private int PageIndex
        {
            get { return ViewState["PageIndex"] as int? ?? 1; }
            set { ViewState["PageIndex"] = value; }
        }

        private int PageSize
        {
            get { return ViewState["PageSize"] as int? ?? 10; }
            set { ViewState["PageSize"] = value; }
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await InitializePageAsync();
            }
        }

        private async Task InitializePageAsync()
        {
            // Get search parameters from query string
            SearchString = Request.QueryString["searchString"] ?? string.Empty;
            SortBy = Request.QueryString["sortBy"] ?? "Name";
            
            if (int.TryParse(Request.QueryString["pageIndex"], out int pageIndex))
            {
                PageIndex = pageIndex;
            }
            
            if (int.TryParse(Request.QueryString["pageSize"], out int pageSize))
            {
                PageSize = pageSize;
            }

            // Initialize search filter control
            SearchFilterControl.SetSearchCriteria(SearchString, SortBy);

            // Check for notification messages
            var notification = SessionManager.Notification;
            if (!string.IsNullOrEmpty(notification))
            {
                ShowNotification(notification);
                SessionManager.ClearNotification();
            }

            // Perform search if there are search criteria
            if (!string.IsNullOrEmpty(SearchString) || Request.QueryString.Count > 0)
            {
                await PerformSearchAsync();
            }
        }

        protected async void SearchFilterControl_SearchRequested(object sender, SearchRequestedEventArgs e)
        {
            SearchString = e.SearchString;
            SortBy = e.SortBy;
            PageIndex = 1; // Reset to first page for new search

            await PerformSearchAsync();
        }

        protected async void PaginationControl_PageChanged(object sender, PageChangedEventArgs e)
        {
            PageIndex = e.NewPageIndex;
            await PerformSearchAsync();
        }

        private async Task PerformSearchAsync()
        {
            try
            {
                // Check if BookService is available
                if (BookService == null)
                {
                    ShowNotification("Search service is not available. Please try again later.");
                    ResultsPanel.Visible = false;
                    NoResultsPanel.Visible = true;
                    return;
                }

                var books = await BookService.GetBooksAsync(SearchString, SortBy, PageIndex, PageSize);

                if (books != null && books.Count > 0)
                {
                    // Show results
                    ResultsPanel.Visible = true;
                    NoResultsPanel.Visible = false;

                    // Update results count
                    var totalResults = books.TotalCount;
                    var startIndex = (PageIndex - 1) * PageSize + 1;
                    var endIndex = Math.Min(PageIndex * PageSize, totalResults);
                    
                    ResultsCountLiteral.Text = $"{startIndex}-{endIndex} of {totalResults}";
                    
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        SearchTermPanel.Visible = true;
                        SearchTermLiteral.Text = SearchString;
                    }

                    // Bind data to controls
                    BookListControl.DataSource = books;
                    BookListControl.DataBind();

                    // Configure pagination
                    PaginationControl.SetPaginationData(books.PageIndex, books.TotalPages, books.HasPreviousPage, books.HasNextPage);
                }
                else
                {
                    // Show no results
                    ResultsPanel.Visible = false;
                    NoResultsPanel.Visible = true;
                }
            }
            catch (Exception ex)
            {
                // Log error and show user-friendly message
                LogError(ex, "Error performing search");
                ShowNotification("An error occurred while searching. Please try again.");
                
                ResultsPanel.Visible = false;
                NoResultsPanel.Visible = true;
            }
        }

        private void ShowNotification(string message)
        {
            NotificationMessage.Text = message;
            notificationPanel.Visible = true;
        }

        private void LogError(Exception ex, string message)
        {
            // Use existing logging infrastructure
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Error(ex, message);
        }
    }
}