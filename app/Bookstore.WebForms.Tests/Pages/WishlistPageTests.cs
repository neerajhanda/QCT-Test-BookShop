using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Books;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web;
using System.Web.SessionState;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class WishlistPageTests
    {
        private Mock<IShoppingCartService> _mockShoppingCartService;
        private Wishlist _wishlistPage;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpSessionStateBase> _mockSession;

        [TestInitialize]
        public void Setup()
        {
            _mockShoppingCartService = new Mock<IShoppingCartService>();
            _wishlistPage = new Wishlist();
            _wishlistPage.ShoppingCartService = _mockShoppingCartService.Object;

            // Setup mock HTTP context
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockSession = new Mock<HttpSessionStateBase>();

            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns("test-correlation-id");
        }

        [TestMethod]
        public async Task LoadWishlistAsync_WithWishlistItems_LoadsItemsSuccessfully()
        {
            // Arrange
            var book1 = new Book { Id = 1, Name = "Test Book 1", CoverImageUrl = "image1.jpg", Price = 19.99m };
            var book2 = new Book { Id = 2, Name = "Test Book 2", CoverImageUrl = "image2.jpg", Price = 24.99m };

            var wishlistItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { Id = 1, Book = book1, IsWishListItem = true },
                new ShoppingCartItem { Id = 2, Book = book2, IsWishListItem = true }
            };

            var shoppingCart = new ShoppingCart();
            // Assuming there's a method to add items to the cart
            foreach (var item in wishlistItems)
            {
                shoppingCart.AddItem(item);
            }

            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync("test-correlation-id"))
                .ReturnsAsync(shoppingCart);

            // Act
            await _wishlistPage.LoadWishlistAsync();

            // Assert
            _mockShoppingCartService.Verify(s => s.GetShoppingCartAsync("test-correlation-id"), Times.Once);
            Assert.IsFalse(_wishlistPage.EmptyWishlistPanel.Visible);
            Assert.IsTrue(_wishlistPage.WishlistPanel.Visible);
        }

        [TestMethod]
        public async Task LoadWishlistAsync_WithEmptyWishlist_ShowsEmptyMessage()
        {
            // Arrange
            var shoppingCart = new ShoppingCart(); // Empty cart

            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync("test-correlation-id"))
                .ReturnsAsync(shoppingCart);

            // Act
            await _wishlistPage.LoadWishlistAsync();

            // Assert
            _mockShoppingCartService.Verify(s => s.GetShoppingCartAsync("test-correlation-id"), Times.Once);
            Assert.IsTrue(_wishlistPage.EmptyWishlistPanel.Visible);
            Assert.IsFalse(_wishlistPage.WishlistPanel.Visible);
        }

        [TestMethod]
        public async Task LoadWishlistAsync_WithNullShoppingCart_ShowsEmptyMessage()
        {
            // Arrange
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync("test-correlation-id"))
                .ReturnsAsync((ShoppingCart)null);

            // Act
            await _wishlistPage.LoadWishlistAsync();

            // Assert
            _mockShoppingCartService.Verify(s => s.GetShoppingCartAsync("test-correlation-id"), Times.Once);
            Assert.IsTrue(_wishlistPage.EmptyWishlistPanel.Visible);
            Assert.IsFalse(_wishlistPage.WishlistPanel.Visible);
        }

        [TestMethod]
        public async Task MoveToShoppingCartAsync_WithValidItem_MovesItemSuccessfully()
        {
            // Arrange
            int shoppingCartItemId = 123;
            _mockShoppingCartService.Setup(s => s.MoveWishlistItemToShoppingCartAsync(It.IsAny<MoveWishlistItemToShoppingCartDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _wishlistPage.MoveToShoppingCartAsync(shoppingCartItemId);

            // Assert
            _mockShoppingCartService.Verify(s => s.MoveWishlistItemToShoppingCartAsync(It.Is<MoveWishlistItemToShoppingCartDto>(dto =>
                dto.CorrelationId == "test-correlation-id" && dto.ShoppingCartItemId == shoppingCartItemId)), Times.Once);
        }

        [TestMethod]
        public async Task MoveAllItemsToShoppingCartAsync_WithValidCart_MovesAllItemsSuccessfully()
        {
            // Arrange
            _mockShoppingCartService.Setup(s => s.MoveAllWishlistItemsToShoppingCartAsync(It.IsAny<MoveAllWishlistItemsToShoppingCartDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _wishlistPage.MoveAllItemsToShoppingCartAsync();

            // Assert
            _mockShoppingCartService.Verify(s => s.MoveAllWishlistItemsToShoppingCartAsync(It.Is<MoveAllWishlistItemsToShoppingCartDto>(dto =>
                dto.CorrelationId == "test-correlation-id")), Times.Once);
        }

        [TestMethod]
        public async Task RemoveFromWishlistAsync_WithValidItem_RemovesItemSuccessfully()
        {
            // Arrange
            int shoppingCartItemId = 123;
            _mockShoppingCartService.Setup(s => s.DeleteShoppingCartItemAsync(It.IsAny<DeleteShoppingCartItemDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _wishlistPage.RemoveFromWishlistAsync(shoppingCartItemId);

            // Assert
            _mockShoppingCartService.Verify(s => s.DeleteShoppingCartItemAsync(It.Is<DeleteShoppingCartItemDto>(dto =>
                dto.CorrelationId == "test-correlation-id" && dto.ShoppingCartItemId == shoppingCartItemId)), Times.Once);
        }

        [TestMethod]
        public async Task MoveToShoppingCartAsync_WithServiceException_HandlesErrorGracefully()
        {
            // Arrange
            int shoppingCartItemId = 123;
            _mockShoppingCartService.Setup(s => s.MoveWishlistItemToShoppingCartAsync(It.IsAny<MoveWishlistItemToShoppingCartDto>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            await _wishlistPage.MoveToShoppingCartAsync(shoppingCartItemId);

            // Assert
            _mockShoppingCartService.Verify(s => s.MoveWishlistItemToShoppingCartAsync(It.IsAny<MoveWishlistItemToShoppingCartDto>()), Times.Once);
            // Error should be handled gracefully
        }
    }
}