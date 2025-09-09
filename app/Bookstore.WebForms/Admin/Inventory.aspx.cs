using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.Books;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Models;

namespace Bookstore.WebForms.Admin
{
    public partial class Inventory : AdminBasePage
    {
        public IBookService BookService { get; set; }
        public IReferenceDataService ReferenceDataService { get; set; }

        private InventoryIndexViewModel _viewModel;
        private BookFilters _currentFilters;

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await InitializePageAsync();
            }
            else
            {
                // Restore filters from ViewState on postback
                _currentFilters = ViewState["CurrentFilters"] as BookFilters ?? new BookFilters();
            }
        }

        private async Task InitializePageAsync()
        {
            try
            {
                // Check for success message from TempData equivalent (Session)
                if (Session["InventoryMessage"] != null)
                {
                    MessageLabel.Text = Session["InventoryMessage"].ToString();
                    MessagePanel.Visible = true;
                    Session.Remove("InventoryMessage");
                }

                // Get filters from query string
                _currentFilters = GetFiltersFromQueryString();
                ViewState["CurrentFilters"] = _currentFilters;

                // Get pagination parameters
                int pageIndex = GetPageIndex();
                int pageSize = GetPageSize();

                await LoadDataAsync(pageIndex, pageSize);
                await LoadReferenceDataAsync();
                BindFilters();
            }
            catch (Exception ex)
            {
                // Log error and show user-friendly message
                ShowErrorMessage("An error occurred while loading the inventory. Please try again.");
                // In a real application, log the exception
            }
        }

        private async Task LoadDataAsync(int pageIndex, int pageSize)
        {
            var books = await BookService.GetBooksAsync(_currentFilters, pageIndex, pageSize);
            var referenceDataItems = await ReferenceDataService.GetAllReferenceDataAsync();

            _viewModel = new InventoryIndexViewModel(books, referenceDataItems);
            
            BindInventoryGrid();
            BindPagination();
        }

        private async Task LoadReferenceDataAsync()
        {
            var referenceDataItems = await ReferenceDataService.GetAllReferenceDataAsync();
            
            // Bind filter dropdowns
            BindDropDown(PublisherFilterDropDown, 
                referenceDataItems.Where(x => x.DataType == ReferenceDataType.Publisher), 
                "All Publishers");
            
            BindDropDown(GenreFilterDropDown, 
                referenceDataItems.Where(x => x.DataType == ReferenceDataType.Genre), 
                "All Genres");
            
            BindDropDown(BookTypeFilterDropDown, 
                referenceDataItems.Where(x => x.DataType == ReferenceDataType.BookType), 
                "All Book Types");
            
            BindDropDown(ConditionFilterDropDown, 
                referenceDataItems.Where(x => x.DataType == ReferenceDataType.Condition), 
                "All Book Conditions");
        }

        private void BindDropDown(DropDownList dropDown, System.Collections.Generic.IEnumerable<ReferenceDataItem> items, string defaultText)
        {
            dropDown.Items.Clear();
            dropDown.Items.Add(new ListItem(defaultText, ""));
            
            foreach (var item in items)
            {
                dropDown.Items.Add(new ListItem(item.Text, item.Id.ToString()));
            }
        }

        private void BindInventoryGrid()
        {
            InventoryGridView.DataSource = _viewModel.Items;
            InventoryGridView.DataBind();
        }

        private void BindPagination()
        {
            // Set pagination data for both controls
            TopPagination.SetPaginationData(_viewModel.CurrentPage, _viewModel.TotalPages, 
                _viewModel.HasPreviousPage, _viewModel.HasNextPage);
            BottomPagination.SetPaginationData(_viewModel.CurrentPage, _viewModel.TotalPages, 
                _viewModel.HasPreviousPage, _viewModel.HasNextPage);
        }

        private void BindFilters()
        {
            NameFilterTextBox.Text = _currentFilters.Name ?? "";
            AuthorFilterTextBox.Text = _currentFilters.Author ?? "";
            LowStockCheckBox.Checked = _currentFilters.LowStock;

            // Set selected values for dropdowns
            if (_currentFilters.PublisherId.HasValue)
                SetDropDownValue(PublisherFilterDropDown, _currentFilters.PublisherId.Value.ToString());
            
            if (_currentFilters.GenreId.HasValue)
                SetDropDownValue(GenreFilterDropDown, _currentFilters.GenreId.Value.ToString());
            
            if (_currentFilters.BookTypeId.HasValue)
                SetDropDownValue(BookTypeFilterDropDown, _currentFilters.BookTypeId.Value.ToString());
            
            if (_currentFilters.ConditionId.HasValue)
                SetDropDownValue(ConditionFilterDropDown, _currentFilters.ConditionId.Value.ToString());
        }

        private void SetDropDownValue(DropDownList dropDown, string value)
        {
            var item = dropDown.Items.FindByValue(value);
            if (item != null)
            {
                dropDown.ClearSelection();
                item.Selected = true;
            }
        }

        private BookFilters GetFiltersFromQueryString()
        {
            var filters = new BookFilters();
            
            if (!string.IsNullOrEmpty(Request.QueryString["name"]))
                filters.Name = Request.QueryString["name"];
            
            if (!string.IsNullOrEmpty(Request.QueryString["author"]))
                filters.Author = Request.QueryString["author"];
            
            if (int.TryParse(Request.QueryString["publisherId"], out int publisherId))
                filters.PublisherId = publisherId;
            
            if (int.TryParse(Request.QueryString["genreId"], out int genreId))
                filters.GenreId = genreId;
            
            if (int.TryParse(Request.QueryString["bookTypeId"], out int bookTypeId))
                filters.BookTypeId = bookTypeId;
            
            if (int.TryParse(Request.QueryString["conditionId"], out int conditionId))
                filters.ConditionId = conditionId;
            
            if (bool.TryParse(Request.QueryString["lowStock"], out bool lowStock))
                filters.LowStock = lowStock;

            return filters;
        }

        private int GetPageIndex()
        {
            if (int.TryParse(Request.QueryString["pageIndex"], out int pageIndex))
                return pageIndex;
            return 1;
        }

        private int GetPageSize()
        {
            if (int.TryParse(Request.QueryString["pageSize"], out int pageSize))
                return pageSize;
            return 10;
        }

        protected async void FilterButton_Click(object sender, EventArgs e)
        {
            // Build filter parameters from form controls
            var queryString = BuildFilterQueryString();
            
            // Redirect to same page with filter parameters
            Response.Redirect($"~/Admin/Inventory.aspx{queryString}");
        }

        protected void ClearButton_Click(object sender, EventArgs e)
        {
            // Redirect to page without any filters
            Response.Redirect("~/Admin/Inventory.aspx");
        }

        private string BuildFilterQueryString()
        {
            var parameters = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrEmpty(NameFilterTextBox.Text))
                parameters.Add($"name={Server.UrlEncode(NameFilterTextBox.Text)}");

            if (!string.IsNullOrEmpty(AuthorFilterTextBox.Text))
                parameters.Add($"author={Server.UrlEncode(AuthorFilterTextBox.Text)}");

            if (!string.IsNullOrEmpty(PublisherFilterDropDown.SelectedValue))
                parameters.Add($"publisherId={PublisherFilterDropDown.SelectedValue}");

            if (!string.IsNullOrEmpty(GenreFilterDropDown.SelectedValue))
                parameters.Add($"genreId={GenreFilterDropDown.SelectedValue}");

            if (!string.IsNullOrEmpty(BookTypeFilterDropDown.SelectedValue))
                parameters.Add($"bookTypeId={BookTypeFilterDropDown.SelectedValue}");

            if (!string.IsNullOrEmpty(ConditionFilterDropDown.SelectedValue))
                parameters.Add($"conditionId={ConditionFilterDropDown.SelectedValue}");

            if (LowStockCheckBox.Checked)
                parameters.Add("lowStock=true");

            return parameters.Count > 0 ? "?" + string.Join("&", parameters) : "";
        }

        protected void InventoryGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (int.TryParse(e.CommandArgument.ToString(), out int bookId))
            {
                switch (e.CommandName)
                {
                    case "ViewDetails":
                        Response.Redirect($"~/Admin/InventoryDetails.aspx?id={bookId}");
                        break;
                    case "UpdateBook":
                        Response.Redirect($"~/Admin/InventoryCreateUpdate.aspx?id={bookId}");
                        break;
                }
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageLabel.Text = message;
            MessagePanel.CssClass = "alert alert-danger mx-3";
            MessagePanel.Visible = true;
        }
    }
}