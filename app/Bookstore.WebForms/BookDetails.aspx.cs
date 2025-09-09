using System;
using System.Threading.Tasks;
using System.Web.UI;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using System.Web;

namespace Bookstore.WebForms
{
    public partial class BookDetails : BasePage
    {
        public IBookService BookService { get; set; }
        public IShoppingCartService ShoppingCartService { get; set; }

        private int BookId
        {
            get { return ViewState["BookId"] as int? ?? 0; }
            set { ViewState["BookId"] = value; }
        }

        private Book CurrentBook
        {
            get { return ViewState["CurrentBook"] as Book; }
            set { ViewState["CurrentBook"] = value; }
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
            // Get book ID from query string
            if (int.TryParse(Request.QueryString["id"], out int bookId))
            {
                BookId = bookId;
                await LoadBookDetailsAsync();
            }
            else
            {
                ShowBookNotFound();
            }

            // Check for notification messages
            var notification = Session["Notification"] as string;
            if (!string.IsNullOrEmpty(notification))
            {
                ShowNotification(notification);
                Session.Remove("Notification");
            }
        }

        private async Task LoadBookDetailsAsync()
        {
            try
            {
                var book = await BookService.GetBookAsync(BookId);
                
                if (book != null)
                {
                    CurrentBook = book;
                    DisplayBookDetails(book);
                }
                else
                {
                    ShowBookNotFound();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error loading book details for ID: {BookId}");
                ShowNotification("An error occurred while loading book details. Please try again.");
                ShowBookNotFound();
            }
        }

        private void DisplayBookDetails(Book book)
        {
            BookDetailsPanel.Visible = true;
            BookNotFoundPanel.Visible = false;

            // Set page title and main heading
            Page.Title = $"{book.Name} - Book Details";
            BookTitleLiteral.Text = book.Name;

            // Set book image
            BookImage.ImageUrl = !string.IsNullOrEmpty(book.CoverImageUrl) ? book.CoverImageUrl : "/Content/images/default_c.jpg";
            BookImage.AlternateText = $"Cover image for {book.Name}";

            // Set book details
            TitleLiteral.Text = book.Name;
            AuthorLiteral.Text = !string.IsNullOrEmpty(book.Author) ? book.Author : "No Author";
            PublisherLiteral.Text = book.Publisher?.Text ?? "Publisher not found";
            ISBNLiteral.Text = !string.IsNullOrEmpty(book.ISBN) ? book.ISBN : "N/A";
            GenreLiteral.Text = book.Genre?.Text ?? "N/A";
            TypeLiteral.Text = book.BookType?.Text ?? "N/A";
            ConditionLiteral.Text = book.Condition?.Text ?? "N/A";
            PriceLiteral.Text = book.Price.ToString("C");

            // Set availability
            if (book.Quantity > 0)
            {
                InStockPanel.Visible = true;
                OutOfStockPanel.Visible = false;
                QuantityLiteral.Text = book.Quantity.ToString();
                ActionButtonsPanel.Visible = true;
            }
            else
            {
                InStockPanel.Visible = false;
                OutOfStockPanel.Visible = true;
                ActionButtonsPanel.Visible = false;
            }

            // Set description
            if (!string.IsNullOrEmpty(book.Summary))
            {
                DescriptionRow.Visible = true;
                NoDescriptionRow.Visible = false;
                DescriptionLiteral.Text = book.Summary;
            }
            else
            {
                DescriptionRow.Visible = false;
                NoDescriptionRow.Visible = true;
            }

            // Set back link with search parameters if available
            var searchString = Request.QueryString["searchString"];
            var sortBy = Request.QueryString["sortBy"];
            var pageIndex = Request.QueryString["pageIndex"];
            
            if (!string.IsNullOrEmpty(searchString) || !string.IsNullOrEmpty(sortBy) || !string.IsNullOrEmpty(pageIndex))
            {
                var backUrl = "~/Search.aspx?";
                var parameters = new System.Collections.Generic.List<string>();
                
                if (!string.IsNullOrEmpty(searchString))
                    parameters.Add($"searchString={HttpUtility.UrlEncode(searchString)}");
                if (!string.IsNullOrEmpty(sortBy))
                    parameters.Add($"sortBy={HttpUtility.UrlEncode(sortBy)}");
                if (!string.IsNullOrEmpty(pageIndex))
                    parameters.Add($"pageIndex={HttpUtility.UrlEncode(pageIndex)}");
                
                BackToSearchLink.NavigateUrl = backUrl + string.Join("&", parameters);
            }
        }

        private void ShowBookNotFound()
        {
            BookDetailsPanel.Visible = false;
            BookNotFoundPanel.Visible = true;
            Page.Title = "Book Not Found";
        }

        protected async void AddToCartButton_Click(object sender, EventArgs e)
        {
            if (CurrentBook == null || CurrentBook.Quantity <= 0)
            {
                ShowNotification("This book is currently out of stock.");
                return;
            }

            try
            {
                var dto = new AddToShoppingCartDto(
                    HttpContext.Current.GetShoppingCartCorrelationId(), 
                    CurrentBook.Id, 
                    1);

                await ShoppingCartService.AddToShoppingCartAsync(dto);
                
                ShowNotification("Item added to shopping cart successfully!");
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error adding book {CurrentBook.Id} to cart");
                ShowNotification("An error occurred while adding the item to your cart. Please try again.");
            }
        }

        protected async void AddToWishlistButton_Click(object sender, EventArgs e)
        {
            if (CurrentBook == null)
            {
                ShowNotification("Unable to add item to wishlist.");
                return;
            }

            try
            {
                var dto = new AddToWishlistDto(
                    HttpContext.Current.GetShoppingCartCorrelationId(), 
                    CurrentBook.Id);

                await ShoppingCartService.AddToWishlistAsync(dto);
                
                ShowNotification("Item added to wishlist successfully!");
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error adding book {CurrentBook.Id} to wishlist");
                ShowNotification("An error occurred while adding the item to your wishlist. Please try again.");
            }
        }

        private void ShowNotification(string message)
        {
            NotificationMessage.Text = message;
            notificationPanel.Visible = true;
        }

        private void LogError(Exception ex, string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Error(ex, message);
        }
    }
}