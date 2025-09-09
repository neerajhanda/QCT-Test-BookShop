using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Bookstore.Domain.Books;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Models;

namespace Bookstore.WebForms.Admin
{
    public partial class InventoryCreateUpdate : AdminBasePage
    {
        public IBookService BookService { get; set; }
        public IReferenceDataService ReferenceDataService { get; set; }

        private InventoryCreateUpdateViewModel _viewModel;
        private bool _isEditMode;
        private int _bookId;

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
                _bookId = GetBookIdFromQueryString();
                _isEditMode = _bookId > 0;

                await LoadReferenceDataAsync();
                
                if (_isEditMode)
                {
                    await LoadBookForEditAsync();
                }
                else
                {
                    InitializeForCreate();
                }

                BindForm();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("An error occurred while loading the page. Please try again.");
                // In a real application, log the exception
            }
        }

        private async Task LoadReferenceDataAsync()
        {
            var referenceDataItems = await ReferenceDataService.GetAllReferenceDataAsync();
            _viewModel = new InventoryCreateUpdateViewModel(referenceDataItems);
        }

        private async Task LoadBookForEditAsync()
        {
            var book = await BookService.GetBookAsync(_bookId);
            if (book == null)
            {
                Response.Redirect("~/Admin/Inventory.aspx");
                return;
            }

            var referenceDataItems = await ReferenceDataService.GetAllReferenceDataAsync();
            _viewModel = new InventoryCreateUpdateViewModel(referenceDataItems, book);
        }

        private void InitializeForCreate()
        {
            // _viewModel is already initialized with reference data
            _viewModel.Quantity = 1; // Default quantity
        }

        private void BindForm()
        {
            if (_viewModel == null) return;

            // Bind basic fields
            BookIdHidden.Value = _viewModel.Id.ToString();
            NameTextBox.Text = _viewModel.Name ?? "";
            AuthorTextBox.Text = _viewModel.Author ?? "";
            ISBNTextBox.Text = _viewModel.ISBN ?? "";
            PriceTextBox.Text = _viewModel.Price.ToString("F2");
            QuantityTextBox.Text = _viewModel.Quantity.ToString();
            YearTextBox.Text = _viewModel.Year > 0 ? _viewModel.Year.ToString() : "";
            SummaryTextBox.Text = _viewModel.Summary ?? "";

            // Bind dropdowns
            BindDropDown(PublisherDropDown, _viewModel.Publishers, "Select the Publisher", _viewModel.SelectedPublisherId);
            BindDropDown(GenreDropDown, _viewModel.Genres, "Select the Genre", _viewModel.SelectedGenreId);
            BindDropDown(BookTypeDropDown, _viewModel.BookTypes, "Select the type", _viewModel.SelectedBookTypeId);
            BindDropDown(ConditionDropDown, _viewModel.BookConditions, "Select the condition", _viewModel.SelectedConditionId);

            // Show existing cover image if in edit mode
            if (_isEditMode && !string.IsNullOrEmpty(_viewModel.CoverImageUrl))
            {
                CoverImagePreview.ImageUrl = _viewModel.CoverImageUrl;
                CoverImagePreview.Visible = true;
            }
        }

        private void BindDropDown(DropDownList dropDown, System.Collections.Generic.List<ListItem> items, string defaultText, int selectedValue = 0)
        {
            dropDown.Items.Clear();
            dropDown.Items.Add(new ListItem(defaultText, ""));
            
            foreach (var item in items)
            {
                dropDown.Items.Add(item);
            }

            if (selectedValue > 0)
            {
                var selectedItem = dropDown.Items.FindByValue(selectedValue.ToString());
                if (selectedItem != null)
                {
                    dropDown.ClearSelection();
                    selectedItem.Selected = true;
                }
            }
        }

        protected async void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                var model = BuildModelFromForm();
                
                BookResult result;
                string successMessage;

                if (_isEditMode)
                {
                    var updateDto = new UpdateBookDto(
                        model.Id,
                        model.Name,
                        model.Author,
                        model.SelectedBookTypeId,
                        model.SelectedConditionId,
                        model.SelectedGenreId,
                        model.SelectedPublisherId,
                        model.Year,
                        model.ISBN,
                        model.Summary,
                        model.Price,
                        model.Quantity,
                        model.CoverImage?.InputStream,
                        model.CoverImage?.FileName);

                    result = await BookService.UpdateAsync(updateDto);
                    successMessage = $"{model.Name} has been updated";
                }
                else
                {
                    var createDto = new CreateBookDto(
                        model.Name,
                        model.Author,
                        model.SelectedBookTypeId,
                        model.SelectedConditionId,
                        model.SelectedGenreId,
                        model.SelectedPublisherId,
                        model.Year,
                        model.ISBN,
                        model.Summary,
                        model.Price,
                        model.Quantity,
                        model.CoverImage?.InputStream,
                        model.CoverImage?.FileName);

                    result = await BookService.AddAsync(createDto);
                    successMessage = $"{model.Name} has been added to inventory";
                }

                if (result.IsSuccess)
                {
                    Session["InventoryMessage"] = successMessage;
                    Response.Redirect("~/Admin/Inventory.aspx");
                }
                else
                {
                    ShowCoverImageError(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("An error occurred while saving the book. Please try again.");
                // In a real application, log the exception
            }
        }

        private InventoryCreateUpdateViewModel BuildModelFromForm()
        {
            var model = new InventoryCreateUpdateViewModel();

            // Parse ID
            if (int.TryParse(BookIdHidden.Value, out int id))
                model.Id = id;

            // Basic fields
            model.Name = NameTextBox.Text.Trim();
            model.Author = AuthorTextBox.Text.Trim();
            model.ISBN = ISBNTextBox.Text.Trim();
            model.Summary = SummaryTextBox.Text.Trim();

            // Parse numeric fields
            if (decimal.TryParse(PriceTextBox.Text, out decimal price))
                model.Price = price;

            if (int.TryParse(QuantityTextBox.Text, out int quantity))
                model.Quantity = quantity;

            if (int.TryParse(YearTextBox.Text, out int year))
                model.Year = year;

            // Parse dropdown selections
            if (int.TryParse(PublisherDropDown.SelectedValue, out int publisherId))
                model.SelectedPublisherId = publisherId;

            if (int.TryParse(GenreDropDown.SelectedValue, out int genreId))
                model.SelectedGenreId = genreId;

            if (int.TryParse(BookTypeDropDown.SelectedValue, out int bookTypeId))
                model.SelectedBookTypeId = bookTypeId;

            if (int.TryParse(ConditionDropDown.SelectedValue, out int conditionId))
                model.SelectedConditionId = conditionId;

            // Handle file upload
            if (CoverImageFileUpload.HasFile)
            {
                model.CoverImage = CoverImageFileUpload.PostedFile;
            }

            return model;
        }

        private int GetBookIdFromQueryString()
        {
            if (int.TryParse(Request.QueryString["id"], out int bookId))
                return bookId;
            return 0;
        }

        private void ShowErrorMessage(string message)
        {
            ErrorLabel.Text = message;
            ErrorPanel.Visible = true;
        }

        private void ShowCoverImageError(string message)
        {
            CoverImageError.Text = message;
            CoverImageError.Visible = true;
        }
    }
}