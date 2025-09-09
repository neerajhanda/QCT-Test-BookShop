using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class ShoppingCartIntegrationTests
    {
        private Mock<IShoppingCartService> _mockShoppingCartService;
        private Mock<IBookService> _mockBookService;

        [TestInitialize]
        public void Setup()
        {
            _mockShoppingCartService = new Mock<IShoppingCartService>();
            _mockBookService = new Mock<IBookService>();
        }

        [TestMethod]
        public async Task AddToCart_BookDetails_UpdatesCartSummary()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var bookId = 123;
            var book = CreateTestBook(bookId, "Test Book", 19.99m, 5);

            _mockBookService
                .Setup(s => s.GetBookAsync(bookId))
                .ReturnsAsync(book);

            _mockShoppingCartService
                .Setup(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()))
                .Returns(Task.CompletedTask);

            var shoppingCart = new Domain.Carts.ShoppingCart(correlationId);
            shoppingCart.AddItemToShoppingCart(bookId, 1);

            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(shoppingCart);

            // Act
            var addToCartDto = new AddToShoppingCartDto(correlationId, bookId, 1);
            await _mockShoppingCartService.Object.AddToShoppingCartAsync(addToCartDto);

            var updatedCart = await _mockShoppingCartService.Object.GetShoppingCartAsync(correlationId);

            // Assert
            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(
                It.Is<AddToShoppingCartDto>(dto => 
                    dto.CorrelationId == correlationId && 
                    dto.BookId == bookId && 
                    dto.Quantity == 1)), Times.Once);

            Assert.IsNotNull(updatedCart);
            Assert.AreEqual(correlationId, updatedCart.CorrelationId);
        }

        [TestMethod]
        public async Task RemoveFromCart_ShoppingCartPage_UpdatesCartSummary()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var shoppingCartItemId = 456;

            _mockShoppingCartService
                .Setup(s => s.DeleteShoppingCartItemAsync(It.IsAny<DeleteShoppingCartItemDto>()))
                .Returns(Task.CompletedTask);

            var emptyCart = new Domain.Carts.ShoppingCart(correlationId);
            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(emptyCart);

            // Act
            var deleteDto = new DeleteShoppingCartItemDto(correlationId, shoppingCartItemId);
            await _mockShoppingCartService.Object.DeleteShoppingCartItemAsync(deleteDto);

            var updatedCart = await _mockShoppingCartService.Object.GetShoppingCartAsync(correlationId);

            // Assert
            _mockShoppingCartService.Verify(s => s.DeleteShoppingCartItemAsync(
                It.Is<DeleteShoppingCartItemDto>(dto => 
                    dto.CorrelationId == correlationId && 
                    dto.ShoppingCartItemId == shoppingCartItemId)), Times.Once);

            Assert.IsNotNull(updatedCart);
            Assert.AreEqual(0, updatedCart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems).Count());
        }

        [TestMethod]
        public async Task AddToWishlist_BookDetails_UpdatesWishlist()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var bookId = 789;

            _mockShoppingCartService
                .Setup(s => s.AddToWishlistAsync(It.IsAny<AddToWishlistDto>()))
                .Returns(Task.CompletedTask);

            var shoppingCart = new Domain.Carts.ShoppingCart(correlationId);
            shoppingCart.AddItemToWishlist(bookId);

            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(shoppingCart);

            // Act
            var addToWishlistDto = new AddToWishlistDto(correlationId, bookId);
            await _mockShoppingCartService.Object.AddToWishlistAsync(addToWishlistDto);

            var updatedCart = await _mockShoppingCartService.Object.GetShoppingCartAsync(correlationId);

            // Assert
            _mockShoppingCartService.Verify(s => s.AddToWishlistAsync(
                It.Is<AddToWishlistDto>(dto => 
                    dto.CorrelationId == correlationId && 
                    dto.BookId == bookId)), Times.Once);

            Assert.IsNotNull(updatedCart);
            Assert.AreEqual(1, updatedCart.GetWishListItems().Count());
        }

        [TestMethod]
        public void CartSummaryControl_EmptyCart_ShowsEmptyMessage()
        {
            // Arrange
            var cartSummaryControl = new CartSummaryControl();
            var emptyCartData = new CartSummaryData
            {
                Items = new List<CartSummaryItem>()
            };

            // Act
            cartSummaryControl.CartData = emptyCartData;

            // Assert
            Assert.AreEqual(0, emptyCartData.Items.Count);
            Assert.AreEqual(0m, emptyCartData.TotalPrice);
        }

        [TestMethod]
        public void CartSummaryControl_WithItems_ShowsCorrectTotal()
        {
            // Arrange
            var cartSummaryControl = new CartSummaryControl();
            var cartData = new CartSummaryData
            {
                Items = new List<CartSummaryItem>
                {
                    new CartSummaryItem
                    {
                        ShoppingCartItemId = 1,
                        BookId = 123,
                        BookName = "Book 1",
                        Price = 19.99m,
                        StockLevel = 5
                    },
                    new CartSummaryItem
                    {
                        ShoppingCartItemId = 2,
                        BookId = 456,
                        BookName = "Book 2",
                        Price = 24.99m,
                        StockLevel = 0
                    }
                }
            };

            // Act
            cartSummaryControl.CartData = cartData;

            // Assert
            Assert.AreEqual(2, cartData.Items.Count);
            Assert.AreEqual(44.98m, cartData.TotalPrice);
        }

        [TestMethod]
        public void CartSummaryItem_StockLevelProperties_SetCorrectly()
        {
            // Arrange & Act
            var lowStockItem = new CartSummaryItem { StockLevel = 3 };
            var outOfStockItem = new CartSummaryItem { StockLevel = 0 };
            var inStockItem = new CartSummaryItem { StockLevel = 10 };

            // Assert
            Assert.IsTrue(lowStockItem.HasLowStockLevels);
            Assert.IsFalse(lowStockItem.IsOutOfStock);

            Assert.IsFalse(outOfStockItem.HasLowStockLevels);
            Assert.IsTrue(outOfStockItem.IsOutOfStock);

            Assert.IsFalse(inStockItem.HasLowStockLevels);
            Assert.IsFalse(inStockItem.IsOutOfStock);
        }

        [TestMethod]
        public async Task ShoppingCartWorkflow_AddRemoveItems_MaintainsConsistency()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var book1Id = 123;
            var book2Id = 456;

            var shoppingCart = new Domain.Carts.ShoppingCart(correlationId);

            // Setup service to return the cart with items added/removed
            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(shoppingCart);

            _mockShoppingCartService
                .Setup(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()))
                .Callback<AddToShoppingCartDto>(dto => shoppingCart.AddItemToShoppingCart(dto.BookId, dto.Quantity))
                .Returns(Task.CompletedTask);

            _mockShoppingCartService
                .Setup(s => s.DeleteShoppingCartItemAsync(It.IsAny<DeleteShoppingCartItemDto>()))
                .Callback<DeleteShoppingCartItemDto>(dto => 
                {
                    var item = shoppingCart.ShoppingCartItems.FirstOrDefault(x => x.Id == dto.ShoppingCartItemId);
                    if (item != null)
                    {
                        shoppingCart.ShoppingCartItems.Remove(item);
                    }
                })
                .Returns(Task.CompletedTask);

            // Act - Add items
            await _mockShoppingCartService.Object.AddToShoppingCartAsync(new AddToShoppingCartDto(correlationId, book1Id, 1));
            await _mockShoppingCartService.Object.AddToShoppingCartAsync(new AddToShoppingCartDto(correlationId, book2Id, 1));

            var cartAfterAdding = await _mockShoppingCartService.Object.GetShoppingCartAsync(correlationId);

            // Act - Remove one item
            var firstItemId = cartAfterAdding.ShoppingCartItems.First().Id;
            await _mockShoppingCartService.Object.DeleteShoppingCartItemAsync(new DeleteShoppingCartItemDto(correlationId, firstItemId));

            var cartAfterRemoving = await _mockShoppingCartService.Object.GetShoppingCartAsync(correlationId);

            // Assert
            Assert.AreEqual(2, cartAfterAdding.ShoppingCartItems.Count);
            Assert.AreEqual(1, cartAfterRemoving.ShoppingCartItems.Count);
        }

        private Book CreateTestBook(int id, string name, decimal price, int quantity)
        {
            return new Book
            {
                Id = id,
                Name = name,
                Price = price,
                Quantity = quantity,
                CoverImageUrl = $"/images/book{id}.jpg",
                Author = "Test Author",
                ISBN = $"978-0-{id:D6}-0",
                Summary = "Test book description"
            };
        }
    }
}