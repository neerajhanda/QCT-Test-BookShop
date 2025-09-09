using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bookstore.WebForms.Tests.Controls
{
    [TestClass]
    public class CartSummaryControlTests
    {
        private CartSummaryControl _control;
        private Page _page;

        [TestInitialize]
        public void Setup()
        {
            _control = new CartSummaryControl();
            _page = new Page();
            _page.Controls.Add(_control);
        }

        [TestMethod]
        public void CartSummaryControl_DefaultProperties_ShouldBeSetCorrectly()
        {
            // Assert
            Assert.IsFalse(_control.ShowLoginPrompt);
            Assert.IsTrue(_control.ShowItemActions);
            Assert.AreEqual(5, _control.MaxDisplayItems);
            Assert.IsNull(_control.CartData);
        }

        [TestMethod]
        public void CartData_WhenSetWithValidData_ShouldStoreCorrectly()
        {
            // Arrange
            var cartData = new CartSummaryData
            {
                Items = new List<CartSummaryItem>
                {
                    new CartSummaryItem
                    {
                        ShoppingCartItemId = 1,
                        BookId = 101,
                        BookName = "Test Book",
                        Price = 19.99m,
                        ImageUrl = "/images/test.jpg",
                        StockLevel = 10
                    }
                }
            };

            // Act
            _control.CartData = cartData;

            // Assert
            Assert.IsNotNull(_control.CartData);
            Assert.AreEqual(1, _control.CartData.Items.Count);
            Assert.AreEqual(19.99m, _control.CartData.TotalPrice);
        }

        [TestMethod]
        public void ShowLoginPrompt_WhenSet_ShouldUpdateProperty()
        {
            // Act
            _control.ShowLoginPrompt = true;

            // Assert
            Assert.IsTrue(_control.ShowLoginPrompt);
        }

        [TestMethod]
        public void ShowItemActions_WhenSet_ShouldUpdateProperty()
        {
            // Act
            _control.ShowItemActions = false;

            // Assert
            Assert.IsFalse(_control.ShowItemActions);
        }

        [TestMethod]
        public void MaxDisplayItems_WhenSet_ShouldUpdateProperty()
        {
            // Act
            _control.MaxDisplayItems = 10;

            // Assert
            Assert.AreEqual(10, _control.MaxDisplayItems);
        }

        [TestMethod]
        public void CartSummaryData_TotalPrice_ShouldCalculateCorrectly()
        {
            // Arrange
            var cartData = new CartSummaryData
            {
                Items = new List<CartSummaryItem>
                {
                    new CartSummaryItem { Price = 19.99m },
                    new CartSummaryItem { Price = 29.99m },
                    new CartSummaryItem { Price = 9.99m }
                }
            };

            // Act & Assert
            Assert.AreEqual(59.97m, cartData.TotalPrice);
        }

        [TestMethod]
        public void CartSummaryData_TotalPrice_WithEmptyItems_ShouldReturnZero()
        {
            // Arrange
            var cartData = new CartSummaryData
            {
                Items = new List<CartSummaryItem>()
            };

            // Act & Assert
            Assert.AreEqual(0m, cartData.TotalPrice);
        }

        [TestMethod]
        public void CartSummaryData_TotalPrice_WithNullItems_ShouldReturnZero()
        {
            // Arrange
            var cartData = new CartSummaryData
            {
                Items = null
            };

            // Act & Assert
            Assert.AreEqual(0m, cartData.TotalPrice);
        }

        [TestMethod]
        public void CartSummaryItem_Properties_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var item = new CartSummaryItem
            {
                ShoppingCartItemId = 123,
                BookId = 456,
                BookName = "Sample Book",
                Price = 24.99m,
                ImageUrl = "/images/sample.jpg",
                StockLevel = 3
            };

            // Assert
            Assert.AreEqual(123, item.ShoppingCartItemId);
            Assert.AreEqual(456, item.BookId);
            Assert.AreEqual("Sample Book", item.BookName);
            Assert.AreEqual(24.99m, item.Price);
            Assert.AreEqual("/images/sample.jpg", item.ImageUrl);
            Assert.AreEqual(3, item.StockLevel);
        }

        [TestMethod]
        public void CartSummaryItem_HasLowStockLevels_ShouldReturnCorrectValue()
        {
            // Test with low stock (5 or less, but greater than 0)
            var item = new CartSummaryItem { StockLevel = 3 };
            Assert.IsTrue(item.HasLowStockLevels);

            item.StockLevel = 5;
            Assert.IsTrue(item.HasLowStockLevels);

            // Test with normal stock
            item.StockLevel = 10;
            Assert.IsFalse(item.HasLowStockLevels);

            // Test with out of stock
            item.StockLevel = 0;
            Assert.IsFalse(item.HasLowStockLevels);
        }

        [TestMethod]
        public void CartSummaryItem_IsOutOfStock_ShouldReturnCorrectValue()
        {
            // Test with out of stock
            var item = new CartSummaryItem { StockLevel = 0 };
            Assert.IsTrue(item.IsOutOfStock);

            // Test with negative stock (edge case)
            item.StockLevel = -1;
            Assert.IsTrue(item.IsOutOfStock);

            // Test with stock available
            item.StockLevel = 1;
            Assert.IsFalse(item.IsOutOfStock);

            item.StockLevel = 10;
            Assert.IsFalse(item.IsOutOfStock);
        }

        [TestMethod]
        public void CartItemRemovedEventArgs_Constructor_ShouldSetItemId()
        {
            // Arrange & Act
            var eventArgs = new CartItemRemovedEventArgs(789);

            // Assert
            Assert.AreEqual(789, eventArgs.ShoppingCartItemId);
        }

        [TestMethod]
        public void CartSummaryControl_ItemRemovedEvent_ShouldBeRaisedCorrectly()
        {
            // Arrange
            var eventRaised = false;
            var removedItemId = 0;
            
            _control.ItemRemoved += (sender, e) =>
            {
                eventRaised = true;
                removedItemId = e.ShoppingCartItemId;
            };

            // Act
            _control.OnItemRemoved(new CartItemRemovedEventArgs(999));

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.AreEqual(999, removedItemId);
        }

        [TestMethod]
        public void CartSummaryControl_CartUpdatedEvent_ShouldBeRaisedCorrectly()
        {
            // Arrange
            var eventRaised = false;
            
            _control.CartUpdated += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act
            _control.OnCartUpdated(EventArgs.Empty);

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void CartSummaryData_WithMultipleItems_ShouldCalculateTotalCorrectly()
        {
            // Arrange
            var cartData = new CartSummaryData
            {
                Items = new List<CartSummaryItem>
                {
                    new CartSummaryItem { Price = 15.50m },
                    new CartSummaryItem { Price = 22.75m },
                    new CartSummaryItem { Price = 8.25m },
                    new CartSummaryItem { Price = 45.00m }
                }
            };

            // Act & Assert
            Assert.AreEqual(91.50m, cartData.TotalPrice);
            Assert.AreEqual(4, cartData.Items.Count);
        }
    }
}