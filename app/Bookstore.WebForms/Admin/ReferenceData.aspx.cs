using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Models;

namespace Bookstore.WebForms.Admin
{
    public partial class ReferenceData : AdminBasePage
    {
        public IReferenceDataService ReferenceDataService { get; set; }

        private AdminReferenceDataIndexViewModel _viewModel;
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
                
                // Check for success message from create/update operations
                if (Session["SuccessMessage"] != null)
                {
                    ShowSuccessMessage(Session["SuccessMessage"].ToString());
                    Session.Remove("SuccessMessage");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading reference data: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            var filters = GetFiltersFromControls();
            var pageIndex = GetCurrentPageIndex();
            
            var referenceDataItems = await ReferenceDataService.GetReferenceDataAsync(filters, pageIndex, DefaultPageSize);

            _viewModel = new AdminReferenceDataIndexViewModel(referenceDataItems, filters);
        }

        private ReferenceDataFilters GetFiltersFromControls()
        {
            return new ReferenceDataFilters
            {
                ReferenceDataType = GetSelectedReferenceDataType()
            };
        }

        private ReferenceDataType? GetSelectedReferenceDataType()
        {
            if (Enum.TryParse<ReferenceDataType>(ReferenceDataTypeFilterDropDown.SelectedValue, out ReferenceDataType type))
                return type;
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

            // Bind filter dropdown
            BindReferenceDataTypeDropDown();

            // Bind grid
            ReferenceDataGridView.DataSource = _viewModel.Items;
            ReferenceDataGridView.DataBind();

            // Restore filter values
            RestoreFilterValues();
        }

        private void BindReferenceDataTypeDropDown()
        {
            ReferenceDataTypeFilterDropDown.Items.Clear();
            ReferenceDataTypeFilterDropDown.Items.Add(new ListItem("All Types", ""));
            ReferenceDataTypeFilterDropDown.Items.AddRange(_viewModel.ReferenceDataTypes.ToArray());
        }

        private void RestoreFilterValues()
        {
            if (_viewModel?.Filters?.ReferenceDataType.HasValue == true)
                ReferenceDataTypeFilterDropDown.SelectedValue = _viewModel.Filters.ReferenceDataType.Value.ToString();
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
            ReferenceDataTypeFilterDropDown.SelectedIndex = 0;
        }

        protected async void ReferenceDataGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int referenceDataId))
            {
                ShowErrorMessage("Invalid reference data ID.");
                return;
            }

            try
            {
                switch (e.CommandName)
                {
                    case "UpdateItem":
                        Response.Redirect($"~/Admin/ReferenceDataCreateUpdate.aspx?id={referenceDataId}");
                        break;
                    case "DeleteItem":
                        await DeleteReferenceDataItem(referenceDataId);
                        break;
                    default:
                        ShowErrorMessage("Invalid action.");
                        return;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error processing request: {ex.Message}");
            }
        }

        private async Task DeleteReferenceDataItem(int referenceDataId)
        {
            try
            {
                // Note: We would need to add a delete method to the service
                // For now, we'll show a message that this functionality needs to be implemented
                ShowErrorMessage("Delete functionality is not yet implemented in the service layer.");
                
                // When implemented, it would look like:
                // await ReferenceDataService.DeleteAsync(referenceDataId);
                // ShowSuccessMessage("Reference data item deleted successfully.");
                // await InitializePageAsync();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting reference data item: {ex.Message}");
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