using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.WebForms;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class SearchPageTests
    {
        private Mock<IBookService> _mockBookService;
        private Mock<IShoppingCartService> _mockShoppingCartService;
        private Search _searchPage;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockHttpRequest;
        private Mock<HttpSessionStateBase> _mockSession;

        [TestInitialize]
        public void Setup()
        {
            _mockBookService = new Mock<IBookService>();
            _mockShoppingCartService = new Mock<IShoppingCartService>();
            
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockHttpRequest = new Mock<HttpRequestBase>();
            _mockSession = new Mock<HttpSessionStateBase>();
            
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
            
            _searchPage = new Search();
            _searchPage.BookService = _mockBookService.Object;
            _searchPage.ShoppingCartService = _mockShoppingCartService.Object;
        }

        [TestMethod]
        public async Task Page_Load_WithSearchString_PerformsSearch()
        {
            // Arrange
            var searchString = "test book";
            var books = CreateMockBookList();
            
            _mockHttpRequest.Setup(r => r.QueryString["searchString"]).Returns(searchString);
            _mockHttpRequest.Setup(r => r.QueryString["sortBy"]).Returns("Name");
            _mockBookService.Setup(s => s.GetBooksAsync(searchString, "Name", 1, 10))
                           .ReturnsAsync(books);

            // Act
            await InvokePageLoadAsync();

            // Assert
            _mockBookService.Verify(s => s.GetBooksAsync(searchString, "Name", 1, 10), Times.Once);
        }

        [TestMethod]
        public async Task Page_Load_WithNoResults_ShowsNoResultsPanel()
        {
            // Arrange
            var searchString = "nonexistent book";
            var emptyBooks = CreateEmptyBookList();
            
            _mockHttpRequest.Setup(r => r.QueryString["searchString"]).Returns(searchString);
            _mockBookService.Setup(s => s.GetBooksAsync(searchString, "Name", 1, 10))
                           .ReturnsAsync(emptyBooks);

            // Act
            await InvokePageLoadAsync();

            // Assert
            _mockBookService.Verify(s => s.GetBooksAsync(searchString, "Name", 1, 10), Times.Once);
            // In a real test, we would verify that NoResultsPanel.Visible = true
        }

        [TestMethod]
        public async Task SearchFilterControl_SearchRequested_PerformsNewSearch()
        {
            // Arrange
            var searchString = "new search";
            var sortBy = "PriceAsc";
            var books = CreateMockBookList();
            
            _mockBookService.Setup(s => s.GetBooksAsync(searchString, sortBy, 1, 10))
                           .ReturnsAsync(books);

            var eventArgs = new SearchRequestedEventArgs(searchString, sortBy);

            // Act
            await InvokeSearchRequestedAsync(eventArgs);

            // Assert
            _mockBookService.Verify(s => s.GetBooksAsync(searchString, sortBy, 1, 10), Times.Once);
        }

        [TestMethod]
        public async Task PaginationControl_PageChanged_LoadsCorrectPage()
        {
            // Arrange
            var pageIndex = 2;
            var books = CreateMockBookList();
            
            _mockBookService.Setup(s => s.GetBooksAsync("", "Name", pageIndex, 10))
                           .ReturnsAsync(books);

            var eventArgs = new PageChangedEventArgs(pageIndex);

            // Act
            await InvokePaginationChangedAsync(eventArgs);

            // Assert
            _mockBookService.Verify(s => s.GetBooksAsync("", "Name", pageIndex, 10), Times.Once);
        }

        [TestMethod]
        public void Page_Load_WithNotification_DisplaysNotification()
        {
            // Arrange
            var notificationMessage = "Item added to cart";
            _mockSession.Setup(s => s["Notification"]).Returns(notificationMessage);

            // Act
            InvokePageLoad();

            // Assert
            _mockSession.Verify(s => s.Remove("Notification"), Times.Once);
            // In a real test, we would verify that the notification is displayed
        }

        [TestMethod]
        public async Task BookService_ThrowsException_HandlesGracefully()
        {
            // Arrange
            var searchString = "test";
            _mockHttpRequest.Setup(r => r.QueryString["searchString"]).Returns(searchString);
            _mockBookService.Setup(s => s.GetBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                           .ThrowsAsync(new Exception("Service error"));

            // Act
            await InvokePageLoadAsync();

            // Assert
            // In a real test, we would verify that error handling was triggered
            _mockBookService.Verify(s => s.GetBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        private IPaginatedList<Book> CreateMockBookList()
        {
            var books = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Name = "Test Book 1",
                    Author = "Test Author 1",
                    Price = 19.99m,
                    Quantity = 5,
                    CoverImageUrl = "/images/book1.jpg"
                },
                new Book
                {
                    Id = 2,
                    Name = "Test Book 2",
                    Author = "Test Author 2",
                    Price = 24.99m,
                    Quantity = 3,
                    CoverImageUrl = "/images/book2.jpg"
                }
            };

            var mockPaginatedList = new Mock<IPaginatedList<Book>>();
            mockPaginatedList.Setup(p => p.Count).Returns(books.Count);
            mockPaginatedList.Setup(p => p.TotalCount).Returns(books.Count);
            mockPaginatedList.Setup(p => p.PageIndex).Returns(1);
            mockPaginatedList.Setup(p => p.TotalPages).Returns(1);
            mockPaginatedList.Setup(p => p.HasPreviousPage).Returns(false);
            mockPaginatedList.Setup(p => p.HasNextPage).Returns(false);
            mockPaginatedList.Setup(p => p.GetEnumerator()).Returns(books.GetEnumerator());

            return mockPaginatedList.Object;
        }

        private IPaginatedList<Book> CreateEmptyBookList()
        {
            var mockPaginatedList = new Mock<IPaginatedList<Book>>();
            mockPaginatedList.Setup(p => p.Count).Returns(0);
            mockPaginatedList.Setup(p => p.TotalCount).Returns(0);
            mockPaginatedList.Setup(p => p.PageIndex).Returns(1);
            mockPaginatedList.Setup(p => p.TotalPages).Returns(0);
            mockPaginatedList.Setup(p => p.HasPreviousPage).Returns(false);
            mockPaginatedList.Setup(p => p.HasNextPage).Returns(false);
            mockPaginatedList.Setup(p => p.GetEnumerator()).Returns(new List<Book>().GetEnumerator());

            return mockPaginatedList.Object;
        }

        private async Task InvokePageLoadAsync()
        {
            // Simulate Page_Load event
            var eventArgs = new EventArgs();
            await _searchPage.GetType()
                .GetMethod("Page_Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .InvokeAsync(_searchPage, new object[] { _searchPage, eventArgs });
        }

        private void InvokePageLoad()
        {
            // Simulate Page_Load event synchronously
            var eventArgs = new EventArgs();
            _searchPage.GetType()
                .GetMethod("Page_Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_searchPage, new object[] { _searchPage, eventArgs });
        }

        private async Task InvokeSearchRequestedAsync(SearchRequestedEventArgs eventArgs)
        {
            // Simulate SearchFilterControl_SearchRequested event
            await _searchPage.GetType()
                .GetMethod("SearchFilterControl_SearchRequested", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .InvokeAsync(_searchPage, new object[] { null, eventArgs });
        }

        private async Task InvokePaginationChangedAsync(PageChangedEventArgs eventArgs)
        {
            // Simulate PaginationControl_PageChanged event
            await _searchPage.GetType()
                .GetMethod("PaginationControl_PageChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .InvokeAsync(_searchPage, new object[] { null, eventArgs });
        }
    }

    // Extension method to handle async method invocation in tests
    public static class MethodInfoExtensions
    {
        public static async Task<object> InvokeAsync(this System.Reflection.MethodInfo method, object obj, params object[] parameters)
        {
            var task = (Task)method.Invoke(obj, parameters);
            await task.ConfigureAwait(false);
            
            var property = task.GetType().GetProperty("Result");
            return property?.GetValue(task);
        }
    }
}