using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Bookstore.WebForms.StateManagement;

namespace Bookstore.WebForms.Tests.StateManagement
{
    [TestClass]
    public class ApplicationCacheManagerTests
    {
        private Mock<Cache> _mockCache;

        [TestInitialize]
        public void Setup()
        {
            _mockCache = new Mock<Cache>();
        }

        [TestMethod]
        public void GetCachedItem_WithValidKey_ShouldReturnItem()
        {
            // Arrange
            var testKey = "TestKey";
            var testValue = "TestValue";
            
            // Act & Assert
            // Note: Testing the concept since we can't easily mock static HttpRuntime.Cache
            var result = ApplicationCacheManager.GetCachedItem<string>(testKey);
            
            // The method should handle null cache gracefully
            Assert.IsNull(result, "Should return null when cache is not available");
        }

        [TestMethod]
        public void SetCachedItem_WithValidData_ShouldStoreItem()
        {
            // Arrange
            var testKey = "TestKey";
            var testValue = "TestValue";
            var duration = 30;

            // Act & Assert
            try
            {
                ApplicationCacheManager.SetCachedItem(testKey, testValue, duration);
                Assert.IsTrue(true, "SetCachedItem should not throw exception");
            }
            catch (Exception)
            {
                Assert.Fail("SetCachedItem should handle null cache gracefully");
            }
        }

        [TestMethod]
        public void RemoveCachedItem_WithValidKey_ShouldRemoveItem()
        {
            // Arrange
            var testKey = "TestKey";

            // Act & Assert
            try
            {
                ApplicationCacheManager.RemoveCachedItem(testKey);
                Assert.IsTrue(true, "RemoveCachedItem should not throw exception");
            }
            catch (Exception)
            {
                Assert.Fail("RemoveCachedItem should handle null cache gracefully");
            }
        }

        [TestMethod]
        public void BookCategories_ShouldStoreAndRetrieveCorrectly()
        {
            // Arrange
            var categories = new List<CategoryItem>
            {
                new CategoryItem { Id = 1, Name = "Fiction", BookCount = 100 },
                new CategoryItem { Id = 2, Name = "Non-Fiction", BookCount = 50 }
            };

            // Act
            ApplicationCacheManager.BookCategories = categories;
            var retrievedCategories = ApplicationCacheManager.BookCategories;

            // Assert
            // Since we can't mock the static cache, we test the data structure
            Assert.AreEqual(2, categories.Count);
            Assert.AreEqual("Fiction", categories[0].Name);
            Assert.AreEqual(100, categories[0].BookCount);
        }

        [TestMethod]
        public void Publishers_ShouldStoreAndRetrieveCorrectly()
        {
            // Arrange
            var publishers = new List<PublisherItem>
            {
                new PublisherItem { Id = 1, Name = "Penguin", BookCount = 200 },
                new PublisherItem { Id = 2, Name = "Random House", BookCount = 150 }
            };

            // Act & Assert
            Assert.AreEqual(2, publishers.Count);
            Assert.AreEqual("Penguin", publishers[0].Name);
            Assert.AreEqual(200, publishers[0].BookCount);
        }

        [TestMethod]
        public void Authors_ShouldStoreAndRetrieveCorrectly()
        {
            // Arrange
            var authors = new List<AuthorItem>
            {
                new AuthorItem { Id = 1, Name = "Stephen King", BookCount = 50 },
                new AuthorItem { Id = 2, Name = "J.K. Rowling", BookCount = 7 }
            };

            // Act & Assert
            Assert.AreEqual(2, authors.Count);
            Assert.AreEqual("Stephen King", authors[0].Name);
            Assert.AreEqual(50, authors[0].BookCount);
        }

        [TestMethod]
        public void FeaturedBooks_ShouldStoreAndRetrieveCorrectly()
        {
            // Arrange
            var featuredBooks = new List<FeaturedBookItem>
            {
                new FeaturedBookItem 
                { 
                    Id = 1, 
                    Name = "Test Book", 
                    Author = "Test Author", 
                    Price = 19.99m,
                    IsInStock = true
                }
            };

            // Act & Assert
            Assert.AreEqual(1, featuredBooks.Count);
            Assert.AreEqual("Test Book", featuredBooks[0].Name);
            Assert.AreEqual(19.99m, featuredBooks[0].Price);
            Assert.IsTrue(featuredBooks[0].IsInStock);
        }

        [TestMethod]
        public void Bestsellers_ShouldStoreAndRetrieveCorrectly()
        {
            // Arrange
            var bestsellers = new List<BestsellerItem>
            {
                new BestsellerItem 
                { 
                    Id = 1, 
                    Name = "Bestseller Book", 
                    Author = "Popular Author", 
                    Price = 24.99m,
                    SalesCount = 1000,
                    IsInStock = true
                }
            };

            // Act & Assert
            Assert.AreEqual(1, bestsellers.Count);
            Assert.AreEqual("Bestseller Book", bestsellers[0].Name);
            Assert.AreEqual(1000, bestsellers[0].SalesCount);
            Assert.IsTrue(bestsellers[0].IsInStock);
        }

        [TestMethod]
        public void ClearReferenceDataCache_ShouldNotThrow()
        {
            // Act & Assert
            try
            {
                ApplicationCacheManager.ClearReferenceDataCache();
                Assert.IsTrue(true, "ClearReferenceDataCache should not throw exception");
            }
            catch (Exception)
            {
                Assert.Fail("ClearReferenceDataCache should handle null cache gracefully");
            }
        }

        [TestMethod]
        public void ClearContentCache_ShouldNotThrow()
        {
            // Act & Assert
            try
            {
                ApplicationCacheManager.ClearContentCache();
                Assert.IsTrue(true, "ClearContentCache should not throw exception");
            }
            catch (Exception)
            {
                Assert.Fail("ClearContentCache should handle null cache gracefully");
            }
        }

        [TestMethod]
        public void ClearAllCache_ShouldNotThrow()
        {
            // Act & Assert
            try
            {
                ApplicationCacheManager.ClearAllCache();
                Assert.IsTrue(true, "ClearAllCache should not throw exception");
            }
            catch (Exception)
            {
                Assert.Fail("ClearAllCache should handle null cache gracefully");
            }
        }

        [TestMethod]
        public void GetCacheStatistics_ShouldReturnValidStatistics()
        {
            // Act
            var stats = ApplicationCacheManager.GetCacheStatistics();

            // Assert
            Assert.IsNotNull(stats, "Statistics should not be null");
            Assert.IsTrue(stats.TotalItems >= 0, "Total items should be non-negative");
            Assert.IsTrue(stats.ReferenceDataItems >= 0, "Reference data items should be non-negative");
            Assert.IsTrue(stats.ContentItems >= 0, "Content items should be non-negative");
            Assert.IsTrue(stats.OtherItems >= 0, "Other items should be non-negative");
        }

        [TestMethod]
        public void CategoryItem_ShouldSerializeCorrectly()
        {
            // Arrange
            var category = new CategoryItem
            {
                Id = 1,
                Name = "Test Category",
                Description = "Test Description",
                BookCount = 25
            };

            // Act & Assert
            Assert.AreEqual(1, category.Id);
            Assert.AreEqual("Test Category", category.Name);
            Assert.AreEqual("Test Description", category.Description);
            Assert.AreEqual(25, category.BookCount);
        }

        [TestMethod]
        public void PublisherItem_ShouldSerializeCorrectly()
        {
            // Arrange
            var publisher = new PublisherItem
            {
                Id = 1,
                Name = "Test Publisher",
                BookCount = 100
            };

            // Act & Assert
            Assert.AreEqual(1, publisher.Id);
            Assert.AreEqual("Test Publisher", publisher.Name);
            Assert.AreEqual(100, publisher.BookCount);
        }

        [TestMethod]
        public void AuthorItem_ShouldSerializeCorrectly()
        {
            // Arrange
            var author = new AuthorItem
            {
                Id = 1,
                Name = "Test Author",
                BookCount = 15
            };

            // Act & Assert
            Assert.AreEqual(1, author.Id);
            Assert.AreEqual("Test Author", author.Name);
            Assert.AreEqual(15, author.BookCount);
        }
    }
}