using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Admin;
using Bookstore.WebForms.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class InventoryPageTests
    {
        private Mock<IBookService> _mockBookService;
        private Mock<IReferenceDataService> _mockReferenceDataService;
        private Inventory _inventoryPage;
        private List<Book> _testBooks;
        private List<ReferenceDataItem> _testReferenceData;

        [TestInitialize]
        public void Setup()
        {
            _mockBookService = new Mock<IBookService>();
            _mockReferenceDataService = new Mock<IReferenceDataService>();
            
            SetupTestData();
            SetupMocks();
            
            _inventoryPage = new Inventory
            {
                BookService = _mockBookService.Object,
                ReferenceDataService = _mockReferenceDataService.Object
            };
        }

        private void SetupTestData()
        {
            _testReferenceData = new List<ReferenceDataItem>
            {
                new ReferenceDataItem { Id = 1, Text = "Fiction", DataType = ReferenceDataType.Genre },
                new ReferenceDataItem { Id = 2, Text = "Non-Fiction", DataType = ReferenceDataType.Genre },
                new ReferenceDataItem { Id = 3, Text = "Penguin", DataType = ReferenceDataType.Publisher },
                new ReferenceDataItem { Id = 4, Text = "Hardcover", DataType = ReferenceDataType.BookType },
                new ReferenceDataItem { Id = 5, Text = "New", DataType = ReferenceDataType.Condition }
            };

            _testBooks = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Name = "Test Book 1",
                    Author = "Test Author 1",
                    Price = 19.99m,
                    Quantity = 5,
                    UpdatedOn = DateTime.Now,
                    Genre = _testReferenceData.First(x => x.DataType == ReferenceDataType.Genre),
                    Publisher = _testReferenceData.First(x => x.DataType == ReferenceDataType.Publisher),
                    BookType = _testReferenceData.First(x => x.DataType == ReferenceDataType.BookType),
                    Condition = _testReferenceData.First(x => x.DataType == ReferenceDataType.Condition)
                },
                new Book
                {
                    Id = 2,
                    Name = "Test Book 2",
                    Author = "Test Author 2",
                    Price = 24.99m,
                    Quantity = 3,
                    UpdatedOn = DateTime.Now,
                    Genre = _testReferenceData.First(x => x.DataType == ReferenceDataType.Genre),
                    Publisher = _testReferenceData.First(x => x.DataType == ReferenceDataType.Publisher),
                    BookType = _testReferenceData.First(x => x.DataType == ReferenceDataType.BookType),
                    Condition = _testReferenceData.First(x => x.DataType == ReferenceDataType.Condition)
                }
            };
        }

        private void SetupMocks()
        {
            var paginatedBooks = new TestPaginatedList<Book>(_testBooks, 1, 10);
            
            _mockBookService.Setup(x => x.GetBooksAsync(It.IsAny<BookFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paginatedBooks);

            _mockReferenceDataService.Setup(x => x.GetAllReferenceDataAsync())
                .ReturnsAsync(_testReferenceData);
        }

        [TestMethod]
        public void InventoryIndexViewModel_Constructor_ShouldInitializeCorrectly()
        {
            // Arrange
            var paginatedBooks = new TestPaginatedList<Book>(_testBooks, 1, 10);

            // Act
            var viewModel = new InventoryIndexViewModel(paginatedBooks, _testReferenceData);

            // Assert
            Assert.AreEqual(2, viewModel.Items.Count);
            Assert.AreEqual("Test Book 1", viewModel.Items[0].Name);
            Assert.AreEqual("Test Author 1", viewModel.Items[0].Author);
            Assert.AreEqual(19.99m, viewModel.Items[0].Price);
            Assert.AreEqual(5, viewModel.Items[0].Quantity);

            Assert.IsTrue(viewModel.Publishers.Any());
            Assert.IsTrue(viewModel.Genres.Any());
            Assert.IsTrue(viewModel.BookTypes.Any());
            Assert.IsTrue(viewModel.BookConditions.Any());
        }

        [TestMethod]
        public void InventoryDetailsViewModel_Constructor_ShouldInitializeCorrectly()
        {
            // Arrange
            var book = _testBooks.First();

            // Act
            var viewModel = new InventoryDetailsViewModel(book);

            // Assert
            Assert.AreEqual(book.Id, viewModel.Id);
            Assert.AreEqual(book.Name, viewModel.Name);
            Assert.AreEqual(book.Author, viewModel.Author);
            Assert.AreEqual(book.Price, viewModel.Price);
            Assert.AreEqual(book.Quantity, viewModel.Quantity);
            Assert.AreEqual(book.Genre.Text, viewModel.Genre);
            Assert.AreEqual(book.Publisher.Text, viewModel.Publisher);
            Assert.AreEqual(book.BookType.Text, viewModel.BookType);
            Assert.AreEqual(book.Condition.Text, viewModel.Condition);
        }

        [TestMethod]
        public void InventoryCreateUpdateViewModel_Constructor_WithReferenceData_ShouldInitializeCorrectly()
        {
            // Act
            var viewModel = new InventoryCreateUpdateViewModel(_testReferenceData);

            // Assert
            Assert.IsTrue(viewModel.Publishers.Any());
            Assert.IsTrue(viewModel.Genres.Any());
            Assert.IsTrue(viewModel.BookTypes.Any());
            Assert.IsTrue(viewModel.BookConditions.Any());
            Assert.AreEqual(1, viewModel.Quantity); // Default quantity
        }

        [TestMethod]
        public void InventoryCreateUpdateViewModel_Constructor_WithBook_ShouldInitializeCorrectly()
        {
            // Arrange
            var book = _testBooks.First();

            // Act
            var viewModel = new InventoryCreateUpdateViewModel(_testReferenceData, book);

            // Assert
            Assert.AreEqual(book.Id, viewModel.Id);
            Assert.AreEqual(book.Name, viewModel.Name);
            Assert.AreEqual(book.Author, viewModel.Author);
            Assert.AreEqual(book.Price, viewModel.Price);
            Assert.AreEqual(book.Quantity, viewModel.Quantity);
            Assert.AreEqual(book.GenreId, viewModel.SelectedGenreId);
            Assert.AreEqual(book.PublisherId, viewModel.SelectedPublisherId);
            Assert.AreEqual(book.BookTypeId, viewModel.SelectedBookTypeId);
            Assert.AreEqual(book.ConditionId, viewModel.SelectedConditionId);
        }

        [TestMethod]
        public void AddReferenceData_ShouldPopulateDropdownLists()
        {
            // Arrange
            var viewModel = new InventoryCreateUpdateViewModel();

            // Act
            viewModel.AddReferenceData(_testReferenceData);

            // Assert
            Assert.AreEqual(1, viewModel.Genres.Count);
            Assert.AreEqual("Fiction", viewModel.Genres.First().Text);
            Assert.AreEqual("1", viewModel.Genres.First().Value);

            Assert.AreEqual(1, viewModel.Publishers.Count);
            Assert.AreEqual("Penguin", viewModel.Publishers.First().Text);
            Assert.AreEqual("3", viewModel.Publishers.First().Value);

            Assert.AreEqual(1, viewModel.BookTypes.Count);
            Assert.AreEqual("Hardcover", viewModel.BookTypes.First().Text);
            Assert.AreEqual("4", viewModel.BookTypes.First().Value);

            Assert.AreEqual(1, viewModel.BookConditions.Count);
            Assert.AreEqual("New", viewModel.BookConditions.First().Text);
            Assert.AreEqual("5", viewModel.BookConditions.First().Value);
        }
    }

    // Helper class for testing pagination
    public class TestPaginatedList<T> : List<T>, IPaginatedList<T>
    {
        public TestPaginatedList(IEnumerable<T> items, int pageIndex, int pageSize)
        {
            AddRange(items);
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = items.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);
        }

        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public IEnumerable<int> GetPageList(int maxPages)
        {
            var pages = new List<int>();
            var startPage = Math.Max(1, PageIndex - maxPages / 2);
            var endPage = Math.Min(TotalPages, startPage + maxPages - 1);

            for (int i = startPage; i <= endPage; i++)
            {
                pages.Add(i);
            }

            return pages;
        }
    }
}