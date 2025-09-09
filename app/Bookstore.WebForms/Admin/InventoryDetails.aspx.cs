using System;
using System.Threading.Tasks;
using Bookstore.Domain.Books;
using Bookstore.WebForms.Models;

namespace Bookstore.WebForms.Admin
{
    public partial class InventoryDetails : AdminBasePage
    {
        public IBookService BookService { get; set; }

        private InventoryDetailsViewModel _viewModel;

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
                int bookId = GetBookIdFromQueryString();
                if (bookId <= 0)
                {
                    Response.Redirect("~/Admin/Inventory.aspx");
                    return;
                }

                await LoadBookDetailsAsync(bookId);
                BindBookDetails();
            }
            catch (Exception ex)
            {
                // Log error and redirect back to inventory
                Response.Redirect("~/Admin/Inventory.aspx");
            }
        }

        private async Task LoadBookDetailsAsync(int bookId)
        {
            var book = await BookService.GetBookAsync(bookId);
            if (book == null)
            {
                Response.Redirect("~/Admin/Inventory.aspx");
                return;
            }

            _viewModel = new InventoryDetailsViewModel(book);
        }

        private void BindBookDetails()
        {
            if (_viewModel == null) return;

            NameLabel.Text = _viewModel.Name;
            AuthorLabel.Text = _viewModel.Author;
            GenreLabel.Text = _viewModel.Genre;
            PriceLabel.Text = _viewModel.Price.ToString("C");
            PublisherLabel.Text = _viewModel.Publisher;
            ISBNLabel.Text = _viewModel.ISBN;
            BookTypeLabel.Text = _viewModel.BookType;
            ConditionLabel.Text = _viewModel.Condition;
            QuantityLabel.Text = _viewModel.Quantity.ToString();
            SummaryLabel.Text = _viewModel.Summary;

            // Set cover image
            if (!string.IsNullOrEmpty(_viewModel.CoverImageUrl))
            {
                CoverImage.ImageUrl = _viewModel.CoverImageUrl;
            }
            else
            {
                CoverImage.ImageUrl = "~/Content/Images/no-image.png"; // Default image
            }

            // Set edit link
            EditLink.NavigateUrl = $"~/Admin/InventoryCreateUpdate.aspx?id={_viewModel.Id}";
        }

        private int GetBookIdFromQueryString()
        {
            if (int.TryParse(Request.QueryString["id"], out int bookId))
                return bookId;
            return 0;
        }
    }
}