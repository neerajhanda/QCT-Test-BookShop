using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bookstore.WebForms.Tests.Controls
{
    [TestClass]
    public class UserControlsIntegrationTests
    {
        [TestMethod]
        public void AllUserControls_ShouldInstantiateSuccessfully()
        {
            // Arrange & Act
            var bookListControl = new BookListControl();
            var paginationControl = new PaginationControl();
            var searchFilterControl = new SearchFilterControl();
            var cartSummaryControl = new CartSummaryControl();

            // Assert
            Assert.IsNotNull(bookListControl);
            Assert.IsNotNull(paginationControl);
            Assert.IsNotNull(searchFilterControl);
            Assert.IsNotNull(cartSummaryControl);
        }

        [TestMethod]
        public void BookListControl_WithSampleData_ShouldHandleDataBinding()
        {
            // Arrange
            var control = new BookListControl();
            var sampleBooks = new List<BookListItem>
            {
                new BookListItem
                {
                    BookId = 1,
                    BookName = "Sample Book 1",
                    BookPrice = 19.99m,
                    CoverImageUrl = "/images/book1.jpg",
                    IsInStock = true,
                    HasLowStockLevels = false
                },
                new BookListItem
                {
                    BookId = 2,
                    BookName = "Sample Book 2",
                    BookPrice = 29.99m,
                    CoverImageUrl = "/images/book2.jpg",
                    IsInStock = false,
                    HasLowStockLevels = false
                }
            };

            // Act
            control.Books = sampleBooks;

            // Assert
            Assert.AreEqual(2, control.Books.Count());
            Assert.AreEqual("Sample Book 1", control.Books.First().BookName);
        }

        [TestMethod]
        public void PaginationControl_WithSampleData_ShouldCalculatePagesCorrectly()
        {
            // Arrange
            var control = new PaginationControl();
            var paginationData = new PaginationData
            {
                CurrentPage = 3,
                TotalPages = 10,
                TotalItems = 100,
                PageSize = 10
            };

            // Act
            control.PaginationData = paginationData;

            // Assert
            Assert.AreEqual(3, control.PaginationData.CurrentPage);
            Assert.IsTrue(control.PaginationData.HasPreviousPage);
            Assert.IsTrue(control.PaginationData.HasNextPage);
        }

        [TestMethod]
        public void SearchFilterControl_WithSampleCriteria_ShouldMaintainState()
        {
            // Arrange
            var control = new SearchFilterControl();
            var criteria = new SearchCriteria
            {
                SearchString = "fantasy",
                SortBy = "PriceAsc",
                MinPrice = 10.00m,
                MaxPrice = 50.00m,
                InStockOnly = true
            };

            // Act
            control.SearchCriteria = criteria;
            var result = control.SearchCriteria;

            // Assert
            Assert.AreEqual("fantasy", result.SearchString);
            Assert.AreEqual("PriceAsc", result.SortBy);
            Assert.AreEqual(10.00m, result.MinPrice);
            Assert.AreEqual(50.00m, result.MaxPrice);
            Assert.IsTrue(result.InStockOnly);
            Assert.IsTrue(result.HasFilters());
        }

        [TestMethod]
        public void CartSummaryControl_WithSampleData_ShouldCalculateTotals()
        {
            // Arrange
            var control = new CartSummaryControl();
            var cartData = new CartSummaryData
            {
                Items = new List<CartSummaryItem>
                {
                    new CartSummaryItem
                    {
                        ShoppingCartItemId = 1,
                        BookId = 101,
                        BookName = "Book 1",
                        Price = 15.99m,
                        StockLevel = 5
                    },
                    new CartSummaryItem
                    {
                        ShoppingCartItemId = 2,
                        BookId = 102,
                        BookName = "Book 2",
                        Price = 24.99m,
                        StockLevel = 0
                    }
                }
            };

            // Act
            control.CartData = cartData;

            // Assert
            Assert.AreEqual(40.98m, control.CartData.TotalPrice);
            Assert.AreEqual(2, control.CartData.Items.Count);
            Assert.IsTrue(control.CartData.Items.First().HasLowStockLevels);
            Assert.IsTrue(control.CartData.Items.Last().IsOutOfStock);
        }

        [TestMethod]
        public void UserControls_EventHandling_ShouldWorkCorrectly()
        {
            // Test PaginationControl events
            var paginationControl = new PaginationControl();
            var pageChangedEventFired = false;
            var newPageNumber = 0;

            paginationControl.PageChanged += (sender, e) =>
            {
                pageChangedEventFired = true;
                newPageNumber = e.NewPageNumber;
            };

            paginationControl.OnPageChanged(new PageChangedEventArgs(5));
            Assert.IsTrue(pageChangedEventFired);
            Assert.AreEqual(5, newPageNumber);

            // Test SearchFilterControl events
            var searchControl = new SearchFilterControl();
            var searchEventFired = false;
            SearchCriteria searchCriteria = null;

            searchControl.SearchRequested += (sender, e) =>
            {
                searchEventFired = true;
                searchCriteria = e.Criteria;
            };

            var testCriteria = new SearchCriteria { SearchString = "test" };
            searchControl.OnSearchRequested(new SearchEventArgs(testCriteria));
            Assert.IsTrue(searchEventFired);
            Assert.AreEqual("test", searchCriteria.SearchString);

            // Test CartSummaryControl events
            var cartControl = new CartSummaryControl();
            var itemRemovedEventFired = false;
            var removedItemId = 0;

            cartControl.ItemRemoved += (sender, e) =>
            {
                itemRemovedEventFired = true;
                removedItemId = e.ShoppingCartItemId;
            };

            cartControl.OnItemRemoved(new CartItemRemovedEventArgs(123));
            Assert.IsTrue(itemRemovedEventFired);
            Assert.AreEqual(123, removedItemId);
        }
    }
}