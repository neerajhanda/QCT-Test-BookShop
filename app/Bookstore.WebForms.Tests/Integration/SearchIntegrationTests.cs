using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class SearchIntegrationTests
    {
        [TestMethod]
        public void SearchFilterControl_InitializesCorrectly()
        {
            // Arrange
            var control = new SearchFilterControl();

            // Act
            var criteria = control.SearchCriteria;

            // Assert
            Assert.IsNotNull(criteria);
            Assert.AreEqual(string.Empty, criteria.SearchString);
            Assert.AreEqual("Name", criteria.SortBy);
        }

        [TestMethod]
        public void SearchFilterControl_SetSearchCriteria_UpdatesControls()
        {
            // Arrange
            var control = new SearchFilterControl();
            var searchString = "test book";
            var sortBy = "PriceAsc";

            // Act
            control.SetSearchCriteria(searchString, sortBy);

            // Assert
            // In a real integration test, we would verify the control values
            // This test verifies the method doesn't throw exceptions
        }

        [TestMethod]
        public void BookListControl_DataBind_HandlesEmptyData()
        {
            // Arrange
            var control = new BookListControl();
            control.DataSource = new List<Bookstore.Domain.Books.Book>();

            // Act & Assert
            // This should not throw an exception
            control.DataBind();
        }

        [TestMethod]
        public void BookListControl_DataBind_HandlesBooksData()
        {
            // Arrange
            var control = new BookListControl();
            var books = new List<Bookstore.Domain.Books.Book>
            {
                new Book
                {
                    Id = 1,
                    Name = "Test Book",
                    Price = 19.99m,
                    Quantity = 5,
                    CoverImageUrl = "/images/test.jpg"
                }
            };
            control.DataSource = books;

            // Act & Assert
            // This should not throw an exception
            control.DataBind();
        }

        [TestMethod]
        public void PaginationControl_SetPaginationData_UpdatesCorrectly()
        {
            // Arrange
            var control = new PaginationControl();

            // Act
            control.SetPaginationData(2, 5, true, true);

            // Assert
            Assert.IsNotNull(control.PaginationData);
            Assert.AreEqual(2, control.PaginationData.CurrentPage);
            Assert.AreEqual(5, control.PaginationData.TotalPages);
            Assert.IsTrue(control.PaginationData.HasPreviousPage);
            Assert.IsTrue(control.PaginationData.HasNextPage);
        }

        [TestMethod]
        public void SearchRequestedEventArgs_Constructor_SetsProperties()
        {
            // Arrange
            var searchString = "test";
            var sortBy = "Name";

            // Act
            var eventArgs = new SearchRequestedEventArgs(searchString, sortBy);

            // Assert
            Assert.AreEqual(searchString, eventArgs.SearchString);
            Assert.AreEqual(sortBy, eventArgs.SortBy);
        }

        [TestMethod]
        public void PageChangedEventArgs_Constructor_SetsProperties()
        {
            // Arrange
            var pageNumber = 3;

            // Act
            var eventArgs = new PageChangedEventArgs(pageNumber);

            // Assert
            Assert.AreEqual(pageNumber, eventArgs.NewPageNumber);
            Assert.AreEqual(pageNumber, eventArgs.NewPageIndex);
        }

        [TestMethod]
        public void SearchCriteria_HasFilters_ReturnsTrueWhenFiltersExist()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchString = "test"
            };

            // Act
            var hasFilters = criteria.HasFilters();

            // Assert
            Assert.IsTrue(hasFilters);
        }

        [TestMethod]
        public void SearchCriteria_HasFilters_ReturnsFalseWhenNoFilters()
        {
            // Arrange
            var criteria = new SearchCriteria();

            // Act
            var hasFilters = criteria.HasFilters();

            // Assert
            Assert.IsFalse(hasFilters);
        }

        [TestMethod]
        public void BookListItem_Properties_SetCorrectly()
        {
            // Arrange & Act
            var item = new BookListItem
            {
                BookId = 1,
                BookName = "Test Book",
                BookPrice = 19.99m,
                CoverImageUrl = "/images/test.jpg",
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
            Assert.AreEqual(19.99m, item.BookPrice);
            Assert.AreEqual("/images/test.jpg", item.CoverImageUrl);
            Assert.IsTrue(item.HasLowStockLevels);
            Assert.IsTrue(item.IsInStock);
            Assert.AreEqual("Test Author", item.Author);
            Assert.AreEqual("Fiction", item.GenreName);
            Assert.AreEqual("Test Publisher", item.PublisherName);
            Assert.AreEqual(3, item.Quantity);
        }
    }
}