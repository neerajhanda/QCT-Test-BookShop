using System;
using System.Threading.Tasks;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class DefaultPageIntegrationTests
    {
        [TestMethod]
        public void DefaultPage_CanBeInstantiated()
        {
            // Arrange & Act
            var page = new Default();

            // Assert
            Assert.IsNotNull(page);
        }

        [TestMethod]
        public void DefaultPage_HasCorrectInheritance()
        {
            // Arrange & Act
            var page = new Default();

            // Assert
            Assert.IsInstanceOfType(page, typeof(BasePage));
        }

        [TestMethod]
        public void HomeIndexItemViewModel_CanBeInstantiated()
        {
            // Arrange & Act
            var viewModel = new HomeIndexItemViewModel
            {
                BookId = 1,
                BookName = "Test Book",
                BookPrice = 19.99m,
                CoverImageUrl = "/images/test.jpg",
                HasLowStockLevels = false,
                IsOutOfStock = false
            };

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(1, viewModel.BookId);
            Assert.AreEqual("Test Book", viewModel.BookName);
            Assert.AreEqual(19.99m, viewModel.BookPrice);
            Assert.AreEqual("/images/test.jpg", viewModel.CoverImageUrl);
            Assert.IsFalse(viewModel.HasLowStockLevels);
            Assert.IsFalse(viewModel.IsOutOfStock);
        }

        [TestMethod]
        public void HomeIndexItemViewModel_PropertiesCanBeSet()
        {
            // Arrange
            var viewModel = new HomeIndexItemViewModel();

            // Act
            viewModel.BookId = 42;
            viewModel.BookName = "Updated Book";
            viewModel.BookPrice = 29.99m;
            viewModel.CoverImageUrl = "/images/updated.jpg";
            viewModel.HasLowStockLevels = true;
            viewModel.IsOutOfStock = true;

            // Assert
            Assert.AreEqual(42, viewModel.BookId);
            Assert.AreEqual("Updated Book", viewModel.BookName);
            Assert.AreEqual(29.99m, viewModel.BookPrice);
            Assert.AreEqual("/images/updated.jpg", viewModel.CoverImageUrl);
            Assert.IsTrue(viewModel.HasLowStockLevels);
            Assert.IsTrue(viewModel.IsOutOfStock);
        }
    }
}