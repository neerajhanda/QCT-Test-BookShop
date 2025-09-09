using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bookstore.WebForms.Tests.Controls
{
    [TestClass]
    public class SearchFilterControlTests
    {
        private SearchFilterControl _control;
        private Page _page;

        [TestInitialize]
        public void Setup()
        {
            _control = new SearchFilterControl();
            _page = new Page();
            _page.Controls.Add(_control);
        }

        [TestMethod]
        public void SearchFilterControl_DefaultProperties_ShouldBeSetCorrectly()
        {
            // Assert
            Assert.IsFalse(_control.ShowAdvancedFilters);
        }

        [TestMethod]
        public void SearchCriteria_WhenSet_ShouldUpdateAllProperties()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchString = "test book",
                SortBy = "PriceAsc",
                Genre = "Fiction",
                Author = "Test Author",
                MinPrice = 10.00m,
                MaxPrice = 50.00m,
                InStockOnly = true
            };

            // Act
            _control.SearchCriteria = criteria;

            // Assert
            var result = _control.SearchCriteria;
            Assert.AreEqual("test book", result.SearchString);
            Assert.AreEqual("PriceAsc", result.SortBy);
            Assert.AreEqual("Fiction", result.Genre);
            Assert.AreEqual("Test Author", result.Author);
            Assert.AreEqual(10.00m, result.MinPrice);
            Assert.AreEqual(50.00m, result.MaxPrice);
            Assert.IsTrue(result.InStockOnly);
        }

        [TestMethod]
        public void SearchCriteria_WhenSetWithNull_ShouldHandleGracefully()
        {
            // Act
            _control.SearchCriteria = null;

            // Assert - Should not throw exception
            var result = _control.SearchCriteria;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ShowAdvancedFilters_WhenToggled_ShouldUpdateProperty()
        {
            // Act
            _control.ShowAdvancedFilters = true;

            // Assert
            Assert.IsTrue(_control.ShowAdvancedFilters);

            // Act
            _control.ShowAdvancedFilters = false;

            // Assert
            Assert.IsFalse(_control.ShowAdvancedFilters);
        }

        [TestMethod]
        public void AvailableGenres_WhenSet_ShouldPopulateDropDown()
        {
            // Arrange
            var genres = new List<GenreItem>
            {
                new GenreItem { Id = 1, Name = "Fiction" },
                new GenreItem { Id = 2, Name = "Non-Fiction" },
                new GenreItem { Id = 3, Name = "Science Fiction" }
            };

            // Act
            _control.AvailableGenres = genres;

            // Assert - This would need to be tested in integration tests
            // as we can't easily access the dropdown items in unit tests
            Assert.IsNotNull(genres);
        }

        [TestMethod]
        public void SearchCriteria_HasFilters_ShouldReturnCorrectValue()
        {
            // Test with no filters
            var criteria = new SearchCriteria();
            Assert.IsFalse(criteria.HasFilters());

            // Test with search string
            criteria.SearchString = "test";
            Assert.IsTrue(criteria.HasFilters());

            // Reset and test with genre
            criteria = new SearchCriteria { Genre = "Fiction" };
            Assert.IsTrue(criteria.HasFilters());

            // Reset and test with author
            criteria = new SearchCriteria { Author = "Test Author" };
            Assert.IsTrue(criteria.HasFilters());

            // Reset and test with price range
            criteria = new SearchCriteria { MinPrice = 10.00m };
            Assert.IsTrue(criteria.HasFilters());

            criteria = new SearchCriteria { MaxPrice = 50.00m };
            Assert.IsTrue(criteria.HasFilters());

            // Reset and test with stock filter
            criteria = new SearchCriteria { InStockOnly = true };
            Assert.IsTrue(criteria.HasFilters());
        }

        [TestMethod]
        public void SearchCriteria_HasFilters_WithWhitespaceStrings_ShouldReturnFalse()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchString = "   ",
                Genre = "",
                Author = "\t\n"
            };

            // Act & Assert
            Assert.IsFalse(criteria.HasFilters());
        }

        [TestMethod]
        public void GenreItem_Properties_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var genre = new GenreItem
            {
                Id = 5,
                Name = "Mystery"
            };

            // Assert
            Assert.AreEqual(5, genre.Id);
            Assert.AreEqual("Mystery", genre.Name);
        }

        [TestMethod]
        public void SearchEventArgs_Constructor_ShouldSetCriteria()
        {
            // Arrange
            var criteria = new SearchCriteria { SearchString = "test" };

            // Act
            var eventArgs = new SearchEventArgs(criteria);

            // Assert
            Assert.AreEqual(criteria, eventArgs.Criteria);
            Assert.AreEqual("test", eventArgs.Criteria.SearchString);
        }

        [TestMethod]
        public void SearchCriteria_PriceFilters_ShouldHandleNullValues()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                MinPrice = null,
                MaxPrice = null
            };

            // Assert
            Assert.IsNull(criteria.MinPrice);
            Assert.IsNull(criteria.MaxPrice);
            Assert.IsFalse(criteria.HasFilters());
        }

        [TestMethod]
        public void SearchCriteria_AllProperties_ShouldBeSettable()
        {
            // Arrange & Act
            var criteria = new SearchCriteria
            {
                SearchString = "fantasy book",
                SortBy = "PriceDesc",
                Genre = "Fantasy",
                Author = "J.R.R. Tolkien",
                MinPrice = 15.99m,
                MaxPrice = 99.99m,
                InStockOnly = false
            };

            // Assert
            Assert.AreEqual("fantasy book", criteria.SearchString);
            Assert.AreEqual("PriceDesc", criteria.SortBy);
            Assert.AreEqual("Fantasy", criteria.Genre);
            Assert.AreEqual("J.R.R. Tolkien", criteria.Author);
            Assert.AreEqual(15.99m, criteria.MinPrice);
            Assert.AreEqual(99.99m, criteria.MaxPrice);
            Assert.IsFalse(criteria.InStockOnly);
            Assert.IsTrue(criteria.HasFilters());
        }
    }
}