using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bookstore.WebForms.Controls
{
    public partial class BookListControl : UserControl
    {
        public enum DisplayMode
        {
            Grid,
            List
        }

        public DisplayMode Mode { get; set; } = DisplayMode.Grid;
        
        public bool ShowPricing { get; set; } = true;
        
        public bool ShowStockStatus { get; set; } = true;
        
        public string DetailsPageUrl { get; set; } = "~/BookDetails.aspx";

        public IEnumerable<BookListItem> Books
        {
            get { return ViewState["Books"] as IEnumerable<BookListItem>; }
            set 
            { 
                ViewState["Books"] = value;
            }
        }

        public object DataSource
        {
            get { return ViewState["DataSource"]; }
            set { ViewState["DataSource"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindBooks();
            }
        }

        public override void DataBind()
        {
            ConvertDataSourceToBooks();
            BindBooks();
            base.DataBind();
        }

        private void BindBooks()
        {
            var books = Books;
            if (books != null && books.Any())
            {
                BooksRepeater.DataSource = books;
                BooksRepeater.DataBind();
                NoResultsPanel.Visible = false;
            }
            else
            {
                BooksRepeater.DataSource = null;
                BooksRepeater.DataBind();
                NoResultsPanel.Visible = true;
            }
        }

        protected void BooksRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var book = (BookListItem)e.Item.DataItem;
                
                // Configure pricing visibility
                var pricePanel = (Panel)e.Item.FindControl("PricePanel");
                var outOfStockPanel = (Panel)e.Item.FindControl("OutOfStockPanel");
                var lowStockPanel = (Panel)e.Item.FindControl("LowStockPanel");
                
                if (!ShowPricing)
                {
                    pricePanel.Visible = false;
                    outOfStockPanel.Visible = false;
                }
                
                if (!ShowStockStatus)
                {
                    lowStockPanel.Visible = false;
                }

                // Apply display mode styling
                var itemDiv = e.Item.FindControl("ItemContainer") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (Mode == DisplayMode.List && itemDiv != null)
                {
                    // Add list-specific CSS classes if needed
                    itemDiv.Attributes["class"] = (itemDiv.Attributes["class"] ?? "") + " list-mode";
                }
            }
        }

        public string GetBookDetailsUrl(object bookId)
        {
            return ResolveUrl($"{DetailsPageUrl}?id={bookId}");
        }

        private void ConvertDataSourceToBooks()
        {
            if (DataSource != null)
            {
                // Convert from Domain.Books.Book to BookListItem
                if (DataSource is IEnumerable<Bookstore.Domain.Books.Book> domainBooks)
                {
                    Books = domainBooks.Select(book => new BookListItem
                    {
                        BookId = book.Id,
                        BookName = book.Name,
                        BookPrice = book.Price,
                        CoverImageUrl = book.CoverImageUrl,
                        HasLowStockLevels = book.Quantity > 0 && book.Quantity <= 5,
                        IsInStock = book.Quantity > 0,
                        Author = book.Author,
                        GenreName = book.Genre?.Text,
                        PublisherName = book.Publisher?.Text,
                        Quantity = book.Quantity
                    }).ToList();
                }
                else if (DataSource is IEnumerable<BookListItem> bookItems)
                {
                    Books = bookItems;
                }
            }
        }
    }

    public class BookListItem
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public decimal BookPrice { get; set; }
        public string CoverImageUrl { get; set; }
        public bool HasLowStockLevels { get; set; }
        public bool IsInStock { get; set; }
        public string Author { get; set; }
        public string GenreName { get; set; }
        public string PublisherName { get; set; }
        public int Quantity { get; set; }
    }
}