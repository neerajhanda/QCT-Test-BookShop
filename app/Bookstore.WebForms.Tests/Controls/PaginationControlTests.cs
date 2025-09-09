using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bookstore.WebForms.Tests.Controls
{
    [TestClass]
    public class PaginationControlTests
    {
        private PaginationControl _control;
        private Page _page;

        [TestInitialize]
        public void Setup()
        {
            _control = new PaginationControl();
            _page = new Page();
            _page.Controls.Add(_control);
        }

        [TestMethod]
        public void PaginationControl_DefaultProperties_ShouldBeSetCorrectly()
        {
            // Assert
            Assert.AreEqual(5, _control.MaxVisiblePages);
            Assert.IsNull(_control.PaginationData);
        }

        [TestMethod]
        public void PaginationData_WhenSetWithValidData_ShouldStoreCorrectly()
        {
            // Arrange
            var paginationData = new PaginationData
            {
                CurrentPage = 2,
                TotalPages = 10,
                TotalItems = 100,
                PageSize = 10
            };

            // Act
            _control.PaginationData = paginationData;

            // Assert
            Assert.IsNotNull(_control.PaginationData);
            Assert.AreEqual(2, _control.PaginationData.CurrentPage);
            Assert.AreEqual(10, _control.PaginationData.TotalPages);
            Assert.AreEqual(100, _control.PaginationData.TotalItems);
            Assert.AreEqual(10, _control.PaginationData.PageSize);
        }

        [TestMethod]
        public void PaginationData_HasPreviousPage_ShouldReturnCorrectValue()
        {
            // Arrange
            var paginationData = new PaginationData { CurrentPage = 2, TotalPages = 5 };

            // Act & Assert
            Assert.IsTrue(paginationData.HasPreviousPage);

            // Test first page
            paginationData.CurrentPage = 1;
            Assert.IsFalse(paginationData.HasPreviousPage);
        }

        [TestMethod]
        public void PaginationData_HasNextPage_ShouldReturnCorrectValue()
        {
            // Arrange
            var paginationData = new PaginationData { CurrentPage = 3, TotalPages = 5 };

            // Act & Assert
            Assert.IsTrue(paginationData.HasNextPage);

            // Test last page
            paginationData.CurrentPage = 5;
            Assert.IsFalse(paginationData.HasNextPage);
        }

        [TestMethod]
        public void MaxVisiblePages_WhenSet_ShouldUpdateProperty()
        {
            // Act
            _control.MaxVisiblePages = 7;

            // Assert
            Assert.AreEqual(7, _control.MaxVisiblePages);
        }

        [TestMethod]
        public void PageButtonData_Properties_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var buttonData = new PageButtonData
            {
                PageNumber = 3,
                IsCurrentPage = true
            };

            // Assert
            Assert.AreEqual(3, buttonData.PageNumber);
            Assert.IsTrue(buttonData.IsCurrentPage);
        }

        [TestMethod]
        public void PageChangedEventArgs_Constructor_ShouldSetPageNumber()
        {
            // Arrange & Act
            var eventArgs = new PageChangedEventArgs(5);

            // Assert
            Assert.AreEqual(5, eventArgs.NewPageNumber);
        }

        [TestMethod]
        public void PaginationData_WithSinglePage_ShouldHaveNoNavigation()
        {
            // Arrange
            var paginationData = new PaginationData
            {
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 5,
                PageSize = 10
            };

            // Assert
            Assert.IsFalse(paginationData.HasPreviousPage);
            Assert.IsFalse(paginationData.HasNextPage);
        }

        [TestMethod]
        public void PaginationData_WithMultiplePages_ShouldCalculateNavigationCorrectly()
        {
            // Arrange
            var paginationData = new PaginationData
            {
                CurrentPage = 3,
                TotalPages = 5,
                TotalItems = 50,
                PageSize = 10
            };

            // Assert
            Assert.IsTrue(paginationData.HasPreviousPage);
            Assert.IsTrue(paginationData.HasNextPage);
        }

        [TestMethod]
        public void PaginationControl_PageChangedEvent_ShouldBeRaisedCorrectly()
        {
            // Arrange
            var eventRaised = false;
            var newPageNumber = 0;
            
            _control.PageChanged += (sender, e) =>
            {
                eventRaised = true;
                newPageNumber = e.NewPageNumber;
            };

            // Act
            _control.OnPageChanged(new PageChangedEventArgs(4));

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.AreEqual(4, newPageNumber);
        }

        [TestMethod]
        public void PaginationData_EdgeCases_ShouldHandleCorrectly()
        {
            // Test with zero pages
            var paginationData = new PaginationData
            {
                CurrentPage = 1,
                TotalPages = 0,
                TotalItems = 0,
                PageSize = 10
            };

            Assert.IsFalse(paginationData.HasPreviousPage);
            Assert.IsFalse(paginationData.HasNextPage);

            // Test with negative values (edge case)
            paginationData.CurrentPage = 0;
            paginationData.TotalPages = -1;
            
            Assert.IsFalse(paginationData.HasPreviousPage);
            Assert.IsFalse(paginationData.HasNextPage);
        }
    }
}