using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Controls
{
    [TestClass]
    public class BookListControlTests
    {
        private BookListControl _control;
        private Page _page;

        [TestInitialize]
        public void Setup()
        {
            _control = new BookListControl();
            _page = new Page();
            _page.Controls.Add(_control);
        }

        [TestMethod]
        public void BookListControl_DefaultProperties_ShouldBeSetCorrectly()
        {
            // Assert
            Assert.AreEqual(BookListControl.DisplayMode.Grid, _control.Mode);
            Assert.IsTrue(_control.ShowPricing);
            Assert.IsTrue(_control.ShowStockStatus);
            Assert.AreEqual("~/BookDetails.aspx", _control.DetailsPageUrl);
        }

        [TestMethod]
        public void Books_WhenSetWithValidData_ShouldStoreInViewState()
        {
            // Arrange
            var books = new List<BookListItem>
            {
                new BookListItem
                {
                    BookId = 1,
                    BookName = "Test Book",
                    BookPrice = 19.99m,
                    CoverImageUrl = "/images/test.jpg",
                    IsInStock = true,
                    HasLowStockLevels = false
                }
            };

            // Act
            _control.Books = books;

            // Assert
            Assert.IsNotNull(_control.Books);
            Assert.AreEqual(1, _control.Books.Count());
            Assert.AreEqual("Test Book", _control.Books.First().BookName);
        }

        [TestMethod]
        public void Books_WhenSetWithEmptyList_ShouldHandleGracefully()
        {
            // Arrange
            var books = new List<BookListItem>();

            // Act
            _control.Books = books;

            // Assert
            Assert.IsNotNull(_control.Books);
            Assert.AreEqual(0, _control.Books.Count());
        }

        [TestMethod]
        public void Books_WhenSetWithNull_ShouldHandleGracefully()
        {
            // Act
            _control.Books = null;

            // Assert - Should not throw exception
            Assert.IsNull(_control.Books);
        }

        [TestMethod]
        public void GetBookDetailsUrl_WithValidBookId_ShouldReturnCorrectUrl()
        {
            // Arrange
            var bookId = 123;

            // Act
            var result = _control.GetBookDetailsUrl(bookId);

            // Assert
            Assert.IsTrue(result.Contains("BookDetails.aspx"));
            Assert.IsTrue(result.Contains("id=123"));
        }

        [TestMethod]
        public void GetBookDetailsUrl_WithCustomDetailsPageUrl_ShouldUseCustomUrl()
        {
            // Arrange
            _control.DetailsPageUrl = "~/CustomDetails.aspx";
            var bookId = 456;

            // Act
            var result = _control.GetBookDetailsUrl(bookId);

            // Assert
            Assert.IsTrue(result.Contains("CustomDetails.aspx"));
            Assert.IsTrue(result.Contains("id=456"));
        }

        [TestMethod]
        public void BookListItem_Properties_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var item = new BookListItem
            {
                BookId = 1,
                BookName = "Test Book",
                BookPrice = 29.99m,
                CoverImageUrl = "/images/book.jpg",
                HasLowStockLevels = true,
                IsInStock = true,
                Author = "Test Author",
                GenreName = "Fiction",
                PublisherName = "Test Publisher",
                Quantity = 3
            };

            // Assert
            Assert.AreEqual(1, item.BookId);
            Assert.AreEqual("Test Book", item.BookName);
            Assert.AreEqual(29.99m, item.BookPrice);
            Assert.AreEqual("/images/book.jpg", item.CoverImageUrl);
            Assert.IsTrue(item.HasLowStockLevels);
            Assert.IsTrue(item.IsInStock);
            Assert.AreEqual("Test Author", item.Author);
            Assert.AreEqual("Fiction", item.GenreName);
            Assert.AreEqual("Test Publisher", item.PublisherName);
            Assert.AreEqual(3, item.Quantity);
        }

        [TestMethod]
        public void Mode_WhenSetToList_ShouldUpdateProperty()
        {
            // Act
            _control.Mode = BookListControl.DisplayMode.List;

            // Assert
            Assert.AreEqual(BookListControl.DisplayMode.List, _control.Mode);
        }

        [TestMethod]
        public void ShowPricing_WhenSetToFalse_ShouldUpdateProperty()
        {
            // Act
            _control.ShowPricing = false;

            // Assert
            Assert.IsFalse(_control.ShowPricing);
        }

        [TestMethod]
        public void ShowStockStatus_WhenSetToFalse_ShouldUpdateProperty()
        {
            // Act
            _control.ShowStockStatus = false;

            // Assert
            Assert.IsFalse(_control.ShowStockStatus);
        }

        [TestMethod]
        public void DetailsPageUrl_WhenSetToCustomValue_ShouldUpdateProperty()
        {
            // Arrange
            var customUrl = "~/Custom/BookDetails.aspx";

            // Act
            _control.DetailsPageUrl = customUrl;

            // Assert
            Assert.AreEqual(customUrl, _control.DetailsPageUrl);
        }
    }
}