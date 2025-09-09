using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.WebForms.Models
{
    public class InventoryIndexViewModel : PaginatedViewModel<InventoryIndexListItemViewModel>
    {
        public BookFilters Filters { get; set; } = new BookFilters();
        public List<ListItem> Publishers { get; set; } = new List<ListItem>();
        public List<ListItem> BookTypes { get; set; } = new List<ListItem>();
        public List<ListItem> Genres { get; set; } = new List<ListItem>();
        public List<ListItem> BookConditions { get; set; } = new List<ListItem>();

        public InventoryIndexViewModel() { }

        public InventoryIndexViewModel(IPaginatedList<Book> books, IEnumerable<ReferenceDataItem> referenceDataItems)
        {
            var items = new List<InventoryIndexListItemViewModel>();
            foreach (var book in books)
            {
                items.Add(new InventoryIndexListItemViewModel
                {
                    Id = book.Id,
                    Name = book.Name,
                    Author = book.Author,
                    BookType = book.BookType.Text,
                    Condition = book.Condition.Text,
                    Genre = book.Genre.Text,
                    Publisher = book.Publisher.Text,
                    UpdatedOn = book.UpdatedOn,
                    Year = book.Year.GetValueOrDefault(),
                    Price = book.Price,
                    Quantity = book.Quantity
                });
            }

            // Set the items using the base class constructor
            Items = items;
            CurrentPage = books.PageIndex;
            ItemsPerPage = books.Count;
            TotalPages = books.TotalPages;
            TotalItems = books.TotalCount;

            // Convert reference data to ListItems for WebForms dropdowns
            BookConditions = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.Condition)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();
            
            BookTypes = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.BookType)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();
            
            Genres = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.Genre)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();
            
            Publishers = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.Publisher)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();
        }
    }

    public class InventoryIndexListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public string Publisher { get; set; }
        public string Genre { get; set; }
        public string BookType { get; set; }
        public string Condition { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class InventoryDetailsViewModel
    {
        public InventoryDetailsViewModel() { }

        public InventoryDetailsViewModel(Book book)
        {
            Author = book.Author;
            BookType = book.BookType.Text;
            Condition = book.Condition.Text;
            CoverImageUrl = book.CoverImageUrl;
            Genre = book.Genre.Text;
            Id = book.Id;
            ISBN = book.ISBN;
            Name = book.Name;
            Price = book.Price;
            Publisher = book.Publisher.Text;
            Quantity = book.Quantity;
            Summary = book.Summary;
            Year = book.Year.GetValueOrDefault();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public string Publisher { get; set; }
        public string BookType { get; set; }
        public string Genre { get; set; }
        public string Condition { get; set; }
        public string CoverImageUrl { get; set; }
        public string Summary { get; set; }
        public string ISBN { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class InventoryCreateUpdateViewModel
    {
        public InventoryCreateUpdateViewModel() { }

        public InventoryCreateUpdateViewModel(IEnumerable<ReferenceDataItem> referenceDataItems)
        {
            AddReferenceData(referenceDataItems);
        }

        public InventoryCreateUpdateViewModel(IEnumerable<ReferenceDataItem> referenceDataItems, Book book) : this(referenceDataItems)
        {
            Author = book.Author;
            CoverImageUrl = book.CoverImageUrl;
            Id = book.Id;
            ISBN = book.ISBN;
            Name = book.Name;
            Price = book.Price;
            Quantity = book.Quantity;
            SelectedBookTypeId = book.BookTypeId;
            SelectedConditionId = book.ConditionId;
            SelectedGenreId = book.GenreId;
            SelectedPublisherId = book.PublisherId;
            Summary = book.Summary;
            Year = book.Year.GetValueOrDefault();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Author { get; set; }

        public int Year { get; set; }

        [Required]
        public string ISBN { get; set; }

        public List<ListItem> Publishers { get; set; } = new List<ListItem>();
        
        [Required]
        [DisplayName("Publisher")]
        public int SelectedPublisherId { get; set; }

        public List<ListItem> BookTypes { get; set; } = new List<ListItem>();

        [Required]
        [DisplayName("Book Type")]
        public int SelectedBookTypeId { get; set; }

        public List<ListItem> Genres { get; set; } = new List<ListItem>();
       
        [Required]
        [DisplayName("Genre")]
        public int SelectedGenreId { get; set; }

        public List<ListItem> BookConditions { get; set; } = new List<ListItem>();
        
        [Required]
        [DisplayName("Condition")]
        public int SelectedConditionId { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [DisplayName("Cover image")]
        public HttpPostedFile CoverImage { get; set; }
        
        public string CoverImageUrl { get; set; }

        public string Summary { get; set; }

        public void AddReferenceData(IEnumerable<ReferenceDataItem> referenceDataItems)
        {
            BookConditions = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.Condition)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();

            BookTypes = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.BookType)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();

            Genres = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.Genre)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();

            Publishers = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.Publisher)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();
        }
    }
}