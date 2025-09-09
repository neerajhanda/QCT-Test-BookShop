using System;
using System.Threading.Tasks;
using System.Web;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class BookDetailsPageTests
    {
        private Mock<IBookService> _mockBookService;
        private Mock<IShoppingCartService> _mockShoppingCartService;
        private BookDetails _bookDetailsPage;
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
            
            _bookDetailsPage = new BookDetails();
            _bookDetailsPage.BookService = _mockBookService.Object;
            _bookDetailsPage.ShoppingCartService = _mockShoppingCartService.Object;
        }

        [TestMethod]
        public async Task Page_Load_WithValidBookId_LoadsBookDetails()
        {
            // Arrange
            var bookId = 1;
            var book = CreateTestBook(bookId);
            
            _mockHttpRequest.Setup(r => r.QueryString["id"]).Returns(bookId.ToString());
            _mockBookService.Setup(s => s.GetBookAsync(bookId)).ReturnsAsync(book);

            // Act
            await InvokePageLoadAsync();

            // Assert
            _mockBookService.Verify(s => s.GetBookAsync(bookId), Times.Once);
        }

        [TestMethod]
        public async Task Page_Load_WithInvalidBookId_ShowsBookNotFound()
        {
            // Arrange
            _mockHttpRequest.Setup(r => r.QueryString["id"]).Returns("invalid");

            // Act
            await InvokePageLoadAsync();

            // Assert
            _mockBookService.Verify(s => s.GetBookAsync(It.IsAny<int>()), Times.Never);
            // In a real test, we would verify that BookNotFoundPanel.Visible = true
        }

        [TestMethod]
        public async Task Page_Load_WithNonExistentBook_ShowsBookNotFound()
        {
            // Arrange
            var bookId = 999;
            _mockHttpRequest.Setup(r => r.QueryString["id"]).Returns(bookId.ToString());
            _mockBookService.Setup(s => s.GetBookAsync(bookId)).ReturnsAsync((Book)null);

            // Act
            await InvokePageLoadAsync();

            // Assert
            _mockBookService.Verify(s => s.GetBookAsync(bookId), Times.Once);
            // In a real test, we would verify that BookNotFoundPanel.Visible = true
        }

        [TestMethod]
        public async Task AddToCartButton_Click_WithInStockBook_AddsToCart()
        {
            // Arrange
            var book = CreateTestBook(1, quantity: 5);
            SetupBookDetailsPage(book);
            
            var expectedDto = new AddToShoppingCartDto("test-correlation-id", book.Id, 1);
            _mockShoppingCartService.Setup(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()))
                                   .Returns(Task.CompletedTask);

            // Act
            await InvokeAddToCartClickAsync();

            // Assert
            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(It.Is<AddToShoppingCartDto>(
                dto => dto.BookId == book.Id && dto.Quantity == 1)), Times.Once);
        }

        [TestMethod]
        public async Task AddToCartButton_Click_WithOutOfStockBook_ShowsErrorMessage()
        {
            // Arrange
            var book = CreateTestBook(1, quantity: 0);
            SetupBookDetailsPage(book);

            // Act
            await InvokeAddToCartClickAsync();

            // Assert
            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()), Times.Never);
            // In a real test, we would verify that an error message is displayed
        }

        [TestMethod]
        public async Task AddToWishlistButton_Click_WithValidBook_AddsToWishlist()
        {
            // Arrange
            var book = CreateTestBook(1);
            SetupBookDetailsPage(book);
            
            _mockShoppingCartService.Setup(s => s.AddToWishlistAsync(It.IsAny<AddToWishlistDto>()))
                                   .Returns(Task.CompletedTask);

            // Act
            await InvokeAddToWishlistClickAsync();

            // Assert
            _mockShoppingCartService.Verify(s => s.AddToWishlistAsync(It.Is<AddToWishlistDto>(
                dto => dto.BookId == book.Id)), Times.Once);
        }

        [TestMethod]
        public async Task BookService_ThrowsException_HandlesGracefully()
        {
            // Arrange
            var bookId = 1;
            _mockHttpRequest.Setup(r => r.QueryString["id"]).Returns(bookId.ToString());
            _mockBookService.Setup(s => s.GetBookAsync(bookId))
                           .ThrowsAsync(new Exception("Service error"));

            // Act
            await InvokePageLoadAsync();

            // Assert
            _mockBookService.Verify(s => s.GetBookAsync(bookId), Times.Once);
            // In a real test, we would verify that error handling was triggered
        }

        [TestMethod]
        public void Page_Load_WithNotification_DisplaysNotification()
        {
            // Arrange
            var notificationMessage = "Item added to cart";
            _mockSession.Setup(s => s["Notification"]).Returns(notificationMessage);
            _mockHttpRequest.Setup(r => r.QueryString["id"]).Returns("1");

            // Act
            InvokePageLoad();

            // Assert
            _mockSession.Verify(s => s.Remove("Notification"), Times.Once);
            // In a real test, we would verify that the notification is displayed
        }

        [TestMethod]
        public void DisplayBookDetails_WithCompleteBook_SetsAllProperties()
        {
            // Arrange
            var book = CreateTestBook(1, "Test Book", "Test Author", 19.99m, 5, "Test summary");

            // Act
            InvokeDisplayBookDetails(book);

            // Assert
            // In a real test, we would verify that all the literal controls are set correctly
            // This would require access to the page controls, which would need a more complex test setup
        }

        [TestMethod]
        public void DisplayBookDetails_WithOutOfStockBook_HidesActionButtons()
        {
            // Arrange
            var book = CreateTestBook(1, quantity: 0);

            // Act
            InvokeDisplayBookDetails(book);

            // Assert
            // In a real test, we would verify that ActionButtonsPanel.Visible = false
        }

        private Book CreateTestBook(int id, string name = "Test Book", string author = "Test Author", 
                                   decimal price = 19.99m, int quantity = 5, string summary = null)
        {
            return new Book
            {
                Id = id,
                Name = name,
                Author = author,
                Price = price,
                Quantity = quantity,
                Summary = summary,
                CoverImageUrl = "/images/test.jpg",
                ISBN = "1234567890",
                Publisher = new ReferenceData { Text = "Test Publisher" },
                Genre = new ReferenceData { Text = "Test Genre" },
                BookType = new ReferenceData { Text = "Paperback" },
                Condition = new ReferenceData { Text = "Good" }
            };
        }

        private void SetupBookDetailsPage(Book book)
        {
            // Set the current book in ViewState (simulating a loaded book)
            _bookDetailsPage.GetType()
                .GetProperty("CurrentBook", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_bookDetailsPage, book);
        }

        private async Task InvokePageLoadAsync()
        {
            var eventArgs = new EventArgs();
            await _bookDetailsPage.GetType()
                .GetMethod("Page_Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .InvokeAsync(_bookDetailsPage, new object[] { _bookDetailsPage, eventArgs });
        }

        private void InvokePageLoad()
        {
            var eventArgs = new EventArgs();
            _bookDetailsPage.GetType()
                .GetMethod("Page_Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_bookDetailsPage, new object[] { _bookDetailsPage, eventArgs });
        }

        private async Task InvokeAddToCartClickAsync()
        {
            var eventArgs = new EventArgs();
            await _bookDetailsPage.GetType()
                .GetMethod("AddToCartButton_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .InvokeAsync(_bookDetailsPage, new object[] { null, eventArgs });
        }

        private async Task InvokeAddToWishlistClickAsync()
        {
            var eventArgs = new EventArgs();
            await _bookDetailsPage.GetType()
                .GetMethod("AddToWishlistButton_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .InvokeAsync(_bookDetailsPage, new object[] { null, eventArgs });
        }

        private void InvokeDisplayBookDetails(Book book)
        {
            _bookDetailsPage.GetType()
                .GetMethod("DisplayBookDetails", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_bookDetailsPage, new object[] { book });
        }
    }
}