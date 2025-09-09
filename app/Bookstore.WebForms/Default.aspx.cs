using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domain.Books;
using NLog;

namespace Bookstore.WebForms
{
    public partial class Default : BasePage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Property injection - this will be automatically injected by the BasePage
        public IBookService BookService { get; set; }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    Logger.Info("Default page loading");

                    await LoadBestSellingBooksAsync();
                    
                    Logger.Info("Default page loaded successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error loading default page");
                    ShowError("An error occurred while loading the page. Please try again.");
                }
            }
        }

        private async Task LoadBestSellingBooksAsync()
        {
            try
            {
                if (BookService == null)
                {
                    Logger.Warn("BookService is null (dependency injection not available), showing placeholder content");
                    ShowNoBooksMessage();
                    return;
                }

                var books = await BookService.ListBestSellingBooksAsync(4);
                var bookViewModels = ConvertToViewModels(books);

                if (bookViewModels.Any())
                {
                    rptBestSellers.DataSource = bookViewModels;
                    rptBestSellers.DataBind();
                    
                    phBestSellers.Visible = true;
                    phNoBooksMessage.Visible = false;
                    
                    Logger.Info($"Loaded {bookViewModels.Count} best selling books");
                }
                else
                {
                    ShowNoBooksMessage();
                    Logger.Info("No best selling books found");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading best selling books");
                ShowNoBooksMessage();
                ShowError("Unable to load featured books at this time.");
            }
        }

        private void ShowNoBooksMessage()
        {
            phBestSellers.Visible = false;
            phNoBooksMessage.Visible = true;
        }

        private List<HomeIndexItemViewModel> ConvertToViewModels(IEnumerable<Book> books)
        {
            if (books == null) return new List<HomeIndexItemViewModel>();

            return books.Select(book => new HomeIndexItemViewModel
            {
                BookId = book.Id,
                CoverImageUrl = book.CoverImageUrl,
                BookPrice = book.Price,
                BookName = book.Name,
                HasLowStockLevels = book.IsLowInStock,
                IsOutOfStock = !book.IsInStock
            }).ToList();
        }
    }

    // ViewModel classes for WebForms data binding
    public class HomeIndexItemViewModel
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public decimal BookPrice { get; set; }
        public string CoverImageUrl { get; set; }
        public bool HasLowStockLevels { get; set; }
        public bool IsOutOfStock { get; set; }
    }
}