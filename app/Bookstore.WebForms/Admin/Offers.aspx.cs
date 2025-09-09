using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Models;

namespace Bookstore.WebForms.Admin
{
    public partial class Offers : AdminBasePage
    {
        public IOfferService OfferService { get; set; }
        public IReferenceDataService ReferenceDataService { get; set; }

        private AdminOffersIndexViewModel _viewModel;
        private const int DefaultPageSize = 10;

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await InitializePageAsync();
            }
        }

        private async Task InitializePageAsync()
        {
            try
            {
                await LoadDataAsync();
                BindControls();
                SetupPagination();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading offers: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            var filters = GetFiltersFromControls();
            var pageIndex = GetCurrentPageIndex();
            
            var offers = await OfferService.GetOffersAsync(filters, pageIndex, DefaultPageSize);
            var referenceData = await ReferenceDataService.GetAllReferenceDataAsync();

            _viewModel = new AdminOffersIndexViewModel(offers, referenceData);
        }

        private OfferFilters GetFiltersFromControls()
        {
            return new OfferFilters
            {
                BookName = BookNameFilterTextBox.Text.Trim(),
                Author = AuthorFilterTextBox.Text.Trim(),
                GenreId = GetSelectedDropDownValue(GenreFilterDropDown),
                ConditionId = GetSelectedDropDownValue(ConditionFilterDropDown),
                OfferStatus = GetSelectedOfferStatus()
            };
        }

        private int? GetSelectedDropDownValue(DropDownList dropDown)
        {
            if (int.TryParse(dropDown.SelectedValue, out int value) && value > 0)
                return value;
            return null;
        }

        private OfferStatus? GetSelectedOfferStatus()
        {
            if (Enum.TryParse<OfferStatus>(OfferStatusFilterDropDown.SelectedValue, out OfferStatus status))
                return status;
            return null;
        }

        private int GetCurrentPageIndex()
        {
            if (int.TryParse(Request.QueryString["page"], out int page) && page > 0)
                return page;
            return 1;
        }

        private void BindControls()
        {
            if (_viewModel == null) return;

            // Bind filter dropdowns
            BindDropDown(GenreFilterDropDown, _viewModel.Genres, "Select Genre");
            BindDropDown(ConditionFilterDropDown, _viewModel.BookConditions, "Select Condition");
            BindDropDown(OfferStatusFilterDropDown, _viewModel.OfferStatuses, "All Statuses");

            // Bind grid
            OffersGridView.DataSource = _viewModel.Items;
            OffersGridView.DataBind();

            // Restore filter values
            RestoreFilterValues();
        }

        private void BindDropDown(DropDownList dropDown, System.Collections.Generic.List<ListItem> items, string defaultText)
        {
            dropDown.Items.Clear();
            dropDown.Items.Add(new ListItem(defaultText, ""));
            dropDown.Items.AddRange(items.ToArray());
        }

        private void RestoreFilterValues()
        {
            if (_viewModel?.Filters == null) return;

            BookNameFilterTextBox.Text = _viewModel.Filters.BookName ?? "";
            AuthorFilterTextBox.Text = _viewModel.Filters.Author ?? "";
            
            if (_viewModel.Filters.GenreId.HasValue)
                GenreFilterDropDown.SelectedValue = _viewModel.Filters.GenreId.Value.ToString();
            
            if (_viewModel.Filters.ConditionId.HasValue)
                ConditionFilterDropDown.SelectedValue = _viewModel.Filters.ConditionId.Value.ToString();
            
            if (_viewModel.Filters.OfferStatus.HasValue)
                OfferStatusFilterDropDown.SelectedValue = _viewModel.Filters.OfferStatus.Value.ToString();
        }

        private void SetupPagination()
        {
            if (_viewModel == null) return;

            TopPagination.SetPaginationData(_viewModel.CurrentPage, _viewModel.TotalPages, 
                _viewModel.HasPreviousPage, _viewModel.HasNextPage);
            BottomPagination.SetPaginationData(_viewModel.CurrentPage, _viewModel.TotalPages, 
                _viewModel.HasPreviousPage, _viewModel.HasNextPage);
        }

        protected async void FilterButton_Click(object sender, EventArgs e)
        {
            await InitializePageAsync();
        }

        protected async void ClearButton_Click(object sender, EventArgs e)
        {
            ClearFilters();
            await InitializePageAsync();
        }

        private void ClearFilters()
        {
            BookNameFilterTextBox.Text = "";
            AuthorFilterTextBox.Text = "";
            GenreFilterDropDown.SelectedIndex = 0;
            ConditionFilterDropDown.SelectedIndex = 0;
            OfferStatusFilterDropDown.SelectedIndex = 0;
        }

        protected async void OffersGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int offerId))
            {
                ShowErrorMessage("Invalid offer ID.");
                return;
            }

            try
            {
                string message = "";
                OfferStatus newStatus;

                switch (e.CommandName)
                {
                    case "Approve":
                        newStatus = OfferStatus.Approved;
                        message = "The offer has been approved";
                        break;
                    case "Reject":
                        newStatus = OfferStatus.Rejected;
                        message = "The offer has been rejected";
                        break;
                    case "Received":
                        newStatus = OfferStatus.Received;
                        message = "The book has been received";
                        break;
                    case "Paid":
                        newStatus = OfferStatus.Paid;
                        message = "The customer has been paid";
                        break;
                    default:
                        ShowErrorMessage("Invalid action.");
                        return;
                }

                var dto = new UpdateOfferStatusDto(offerId, newStatus);
                await OfferService.UpdateOfferStatusAsync(dto);

                ShowSuccessMessage(message);
                await InitializePageAsync();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error updating offer status: {ex.Message}");
            }
        }

        private void ShowSuccessMessage(string message)
        {
            MessageLabel.Text = message;
            MessagePanel.CssClass = "alert alert-success mx-3";
            MessagePanel.Visible = true;
        }

        private void ShowErrorMessage(string message)
        {
            MessageLabel.Text = message;
            MessagePanel.CssClass = "alert alert-danger mx-3";
            MessagePanel.Visible = true;
        }
    }
}