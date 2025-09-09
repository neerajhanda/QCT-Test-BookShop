using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Models;

namespace Bookstore.WebForms.Admin
{
    public partial class ReferenceDataCreateUpdate : AdminBasePage
    {
        public IReferenceDataService ReferenceDataService { get; set; }

        private AdminReferenceDataCreateUpdateViewModel _viewModel;
        private int? _referenceDataId;

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
                _referenceDataId = GetReferenceDataIdFromQueryString();
                
                if (_referenceDataId.HasValue)
                {
                    await LoadExistingReferenceDataAsync();
                }
                else
                {
                    LoadNewReferenceData();
                }

                BindControls();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading reference data: {ex.Message}");
            }
        }

        private int? GetReferenceDataIdFromQueryString()
        {
            if (int.TryParse(Request.QueryString["id"], out int id) && id > 0)
                return id;
            return null;
        }

        private async Task LoadExistingReferenceDataAsync()
        {
            var referenceDataItem = await ReferenceDataService.GetReferenceDataItemAsync(_referenceDataId.Value);
            _viewModel = new AdminReferenceDataCreateUpdateViewModel(referenceDataItem);
        }

        private void LoadNewReferenceData()
        {
            _viewModel = new AdminReferenceDataCreateUpdateViewModel();
            
            // Check if a specific reference data type was requested
            if (Enum.TryParse<ReferenceDataType>(Request.QueryString["type"], out ReferenceDataType selectedType))
            {
                _viewModel.SelectedReferenceDataType = selectedType;
            }
        }

        private void BindControls()
        {
            if (_viewModel == null) return;

            // Set page title and button text
            PageTitleLabel.Text = _viewModel.PageTitle;
            SubmitButton.Text = _viewModel.SubmitButtonText;

            // Bind reference data type dropdown
            ReferenceDataTypeDropDown.Items.Clear();
            ReferenceDataTypeDropDown.Items.Add(new ListItem("Select Type", ""));
            ReferenceDataTypeDropDown.Items.AddRange(_viewModel.DataTypes.ToArray());

            // Set form values
            if (_viewModel.IsEditMode)
            {
                ReferenceDataTypeDropDown.SelectedValue = ((int)_viewModel.SelectedReferenceDataType).ToString();
                TextTextBox.Text = _viewModel.Text;
            }
            else if (_viewModel.SelectedReferenceDataType != default(ReferenceDataType))
            {
                ReferenceDataTypeDropDown.SelectedValue = ((int)_viewModel.SelectedReferenceDataType).ToString();
            }
        }

        protected async void SubmitButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                if (_referenceDataId.HasValue)
                {
                    await UpdateReferenceDataAsync();
                }
                else
                {
                    await CreateReferenceDataAsync();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving reference data: {ex.Message}");
            }
        }

        private async Task CreateReferenceDataAsync()
        {
            var selectedType = (ReferenceDataType)Enum.Parse(typeof(ReferenceDataType), ReferenceDataTypeDropDown.SelectedValue);
            var dto = new CreateReferenceDataItemDto(selectedType, TextTextBox.Text.Trim());

            await ReferenceDataService.CreateAsync(dto);

            Session["SuccessMessage"] = "Reference data created successfully.";
            Response.Redirect("~/Admin/ReferenceData.aspx");
        }

        private async Task UpdateReferenceDataAsync()
        {
            var selectedType = (ReferenceDataType)Enum.Parse(typeof(ReferenceDataType), ReferenceDataTypeDropDown.SelectedValue);
            var dto = new UpdateReferenceDataItemDto(_referenceDataId.Value, selectedType, TextTextBox.Text.Trim());

            await ReferenceDataService.UpdateAsync(dto);

            Session["SuccessMessage"] = "Reference data updated successfully.";
            Response.Redirect("~/Admin/ReferenceData.aspx");
        }

        private void ShowErrorMessage(string message)
        {
            MessageLabel.Text = message;
            MessagePanel.CssClass = "alert alert-danger mx-3";
            MessagePanel.Visible = true;
        }
    }
}