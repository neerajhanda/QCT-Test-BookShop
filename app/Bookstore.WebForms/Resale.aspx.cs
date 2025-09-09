using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.Domain;
using System.IO;

namespace Bookstore.WebForms
{
    public partial class Resale : BasePage
    {
        public IOfferService OfferService { get; set; }
        public IReferenceDataService ReferenceDataService { get; set; }
        public IFileService FileService { get; set; }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadPageAsync();
            }
        }

        private async Task LoadPageAsync()
        {
            try
            {
                // Require authentication for resale functionality
                RequireAuthentication();

                await LoadOffersAsync();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while loading the page. Please try again.");
            }
        }

        private async Task LoadOffersAsync()
        {
            try
            {
                var userSub = CurrentUserId;
                if (string.IsNullOrEmpty(userSub))
                {
                    Response.Redirect("~/Login.aspx?returnUrl=" + Server.UrlEncode(Request.Url.ToString()));
                    return;
                }

                var offers = await OfferService.GetOffersAsync(userSub);
                var offerList = offers.ToList();

                if (!offerList.Any())
                {
                    NoOffersPanel.Visible = true;
                    OffersPanel.Visible = false;
                }
                else
                {
                    NoOffersPanel.Visible = false;
                    OffersPanel.Visible = true;
                    
                    var offerViewModels = offerList.Select(x => new
                    {
                        BookName = x.BookName,
                        Author = x.Author,
                        Genre = x.Genre?.Text ?? "N/A",
                        Publisher = x.Publisher?.Text ?? "N/A",
                        BookType = x.BookType?.Text ?? "N/A",
                        ISBN = x.ISBN,
                        Condition = x.Condition?.Text ?? "N/A",
                        Price = x.BookPrice,
                        OfferStatus = x.OfferStatus.GetDescription()
                    }).ToList();

                    OffersGridView.DataSource = offerViewModels;
                    OffersGridView.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while loading your offers. Please try again.");
            }
        }

        protected void CreateOfferButton_Click(object sender, EventArgs e)
        {
            ShowCreateOfferForm();
        }

        private async void ShowCreateOfferForm()
        {
            try
            {
                CreateOfferPanel.Visible = true;
                await LoadReferenceDataAsync();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while loading the form. Please try again.");
            }
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                var referenceDataItems = await ReferenceDataService.GetAllReferenceDataAsync();
                var referenceDataList = referenceDataItems.ToList();

                // Load Book Types
                var bookTypes = referenceDataList
                    .Where(x => x.DataType == ReferenceDataType.BookType)
                    .Select(x => new ListItem(x.Text, x.Id.ToString()))
                    .ToList();
                BookTypeDropDownList.Items.Clear();
                BookTypeDropDownList.Items.Add(new ListItem("Select the book type", ""));
                BookTypeDropDownList.Items.AddRange(bookTypes.ToArray());

                // Load Publishers
                var publishers = referenceDataList
                    .Where(x => x.DataType == ReferenceDataType.Publisher)
                    .Select(x => new ListItem(x.Text, x.Id.ToString()))
                    .ToList();
                PublisherDropDownList.Items.Clear();
                PublisherDropDownList.Items.Add(new ListItem("Select the publisher", ""));
                PublisherDropDownList.Items.AddRange(publishers.ToArray());

                // Load Genres
                var genres = referenceDataList
                    .Where(x => x.DataType == ReferenceDataType.Genre)
                    .Select(x => new ListItem(x.Text, x.Id.ToString()))
                    .ToList();
                GenreDropDownList.Items.Clear();
                GenreDropDownList.Items.Add(new ListItem("Select the genre", ""));
                GenreDropDownList.Items.AddRange(genres.ToArray());

                // Load Conditions
                var conditions = referenceDataList
                    .Where(x => x.DataType == ReferenceDataType.Condition)
                    .Select(x => new ListItem(x.Text, x.Id.ToString()))
                    .ToList();
                ConditionDropDownList.Items.Clear();
                ConditionDropDownList.Items.Add(new ListItem("Select the book condition", ""));
                ConditionDropDownList.Items.AddRange(conditions.ToArray());
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while loading reference data. Please try again.");
            }
        }

        protected async void SubmitOfferButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                var userSub = CurrentUserId;
                if (string.IsNullOrEmpty(userSub))
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                // Handle file upload if provided
                string uploadedFilePath = null;
                if (BookCoverFileUpload.HasFile)
                {
                    uploadedFilePath = await HandleFileUploadAsync();
                    if (uploadedFilePath == null)
                    {
                        // Error occurred during file upload, message already shown
                        return;
                    }
                }

                // Parse form values
                if (!decimal.TryParse(BookPriceTextBox.Text, out decimal bookPrice))
                {
                    ShowError("Please enter a valid book price.");
                    return;
                }

                if (!int.TryParse(BookTypeDropDownList.SelectedValue, out int bookTypeId) ||
                    !int.TryParse(PublisherDropDownList.SelectedValue, out int publisherId) ||
                    !int.TryParse(GenreDropDownList.SelectedValue, out int genreId) ||
                    !int.TryParse(ConditionDropDownList.SelectedValue, out int conditionId))
                {
                    ShowError("Please select all required dropdown values.");
                    return;
                }

                // Create the offer
                var dto = new CreateOfferDto(
                    userSub,
                    BookNameTextBox.Text.Trim(),
                    AuthorTextBox.Text.Trim(),
                    ISBNTextBox.Text.Trim(),
                    bookTypeId,
                    conditionId,
                    genreId,
                    publisherId,
                    bookPrice);

                await OfferService.CreateOfferAsync(dto);

                ShowSuccess("Your book offer has been submitted successfully!");
                
                // Reset form and reload offers
                ResetCreateOfferForm();
                CreateOfferPanel.Visible = false;
                await LoadOffersAsync();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while submitting your offer. Please try again.");
            }
        }

        private async Task<string> HandleFileUploadAsync()
        {
            try
            {
                if (!BookCoverFileUpload.HasFile)
                    return null;

                var file = BookCoverFileUpload.PostedFile;
                
                // Validate file size (5MB max)
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    ShowError("File size must be less than 5MB.");
                    return null;
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ShowError("Please upload a valid image file (JPG, PNG, GIF).");
                    return null;
                }

                // Generate unique filename
                var fileName = $"book_cover_{Guid.NewGuid()}{fileExtension}";
                
                // Upload to file service (AWS S3)
                using (var stream = file.InputStream)
                {
                    var filePath = await FileService.SaveAsync(stream, fileName);
                    return filePath;
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while uploading the file. Please try again.");
                return null;
            }
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            ResetCreateOfferForm();
            CreateOfferPanel.Visible = false;
        }

        private void ResetCreateOfferForm()
        {
            BookNameTextBox.Text = string.Empty;
            AuthorTextBox.Text = string.Empty;
            ISBNTextBox.Text = string.Empty;
            BookPriceTextBox.Text = string.Empty;
            BookTypeDropDownList.SelectedIndex = 0;
            PublisherDropDownList.SelectedIndex = 0;
            GenreDropDownList.SelectedIndex = 0;
            ConditionDropDownList.SelectedIndex = 0;
        }

        private void ShowError(string message)
        {
            MessagePanel.Visible = true;
            MessageLabel.Text = $"<div class='alert alert-danger'>{message}</div>";
        }

        private void ShowSuccess(string message)
        {
            MessagePanel.Visible = true;
            MessageLabel.Text = $"<div class='alert alert-success'>{message}</div>";
        }
    }
}