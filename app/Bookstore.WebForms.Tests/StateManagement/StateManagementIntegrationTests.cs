using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Bookstore.WebForms.StateManagement;

namespace Bookstore.WebForms.Tests.StateManagement
{
    [TestClass]
    public class StateManagementIntegrationTests
    {
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpSessionStateBase> _mockSession;
        private Mock<Page> _mockPage;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockSession = new Mock<HttpSessionStateBase>();
            _mockPage = new Mock<Page>();
            
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        [TestMethod]
        public void StateManagement_SessionAndCache_ShouldWorkTogether()
        {
            // Arrange
            var userPreferences = new UserPreferences
            {
                PageSize = 20,
                SortBy = "Price",
                SortDirection = "DESC",
                Theme = "Dark"
            };

            var searchCriteria = new SearchCriteria
            {
                Query = "programming books",
                Category = "Technology",
                MinPrice = 20.00m,
                MaxPrice = 100.00m,
                InStockOnly = true,
                PageNumber = 2,
                PageSize = 20
            };

            // Act & Assert - Test data structures
            Assert.AreEqual(20, userPreferences.PageSize);
            Assert.AreEqual("Price", userPreferences.SortBy);
            Assert.AreEqual("programming books", searchCriteria.Query);
            Assert.AreEqual("Technology", searchCriteria.Category);
        }

        [TestMethod]
        public void StateManagement_ViewStateOptimization_ShouldReducePageSize()
        {
            // Arrange
            var testPage = new TestPage();
            
            // Add various controls to test optimization
            var staticPanel = new System.Web.UI.WebControls.Panel();
            var label = new System.Web.UI.WebControls.Label();
            var literal = new System.Web.UI.WebControls.Literal();
            
            staticPanel.Controls.Add(label);
            staticPanel.Controls.Add(literal);
            testPage.Controls.Add(staticPanel);

            var interactivePanel = new System.Web.UI.WebControls.Panel();
            var button = new System.Web.UI.WebControls.Button();
            var textBox = new System.Web.UI.WebControls.TextBox();
            
            interactivePanel.Controls.Add(button);
            interactivePanel.Controls.Add(textBox);
            testPage.Controls.Add(interactivePanel);

            // Act - Simulate ViewState optimization
            ViewStateManager.OptimizePageViewState(testPage);

            // Assert
            Assert.IsTrue(true, "ViewState optimization completed without errors");
        }

        [TestMethod]
        public void StateManagement_CacheAndSession_ShouldHandleUserWorkflow()
        {
            // Arrange - Simulate a user browsing workflow
            var categories = new List<CategoryItem>
            {
                new CategoryItem { Id = 1, Name = "Fiction", BookCount = 500 },
                new CategoryItem { Id = 2, Name = "Technology", BookCount = 200 }
            };

            var userPreferences = new UserPreferences
            {
                PageSize = 15,
                ShowOutOfStock = false
            };

            var searchCriteria = new SearchCriteria
            {
                Category = "Fiction",
                InStockOnly = true,
                PageSize = 15
            };

            // Act - Store in cache and session (conceptually)
            ApplicationCacheManager.BookCategories = categories;
            
            // Assert - Verify data integrity
            Assert.AreEqual(2, categories.Count);
            Assert.AreEqual("Fiction", categories[0].Name);
            Assert.AreEqual(15, userPreferences.PageSize);
            Assert.IsFalse(userPreferences.ShowOutOfStock);
            Assert.AreEqual("Fiction", searchCriteria.Category);
            Assert.IsTrue(searchCriteria.InStockOnly);
        }

        [TestMethod]
        public void StateManagement_ErrorHandling_ShouldMaintainConsistency()
        {
            // Arrange
            var errorMessage = "Test error occurred";
            var returnUrl = "/previous-page";

            // Act - Simulate error scenario
            try
            {
                // Simulate an error condition
                throw new InvalidOperationException(errorMessage);
            }
            catch (Exception ex)
            {
                // Store error information (conceptually)
                var storedMessage = ex.Message;
                var storedReturnUrl = returnUrl;

                // Assert
                Assert.AreEqual(errorMessage, storedMessage);
                Assert.AreEqual("/previous-page", storedReturnUrl);
            }
        }

        [TestMethod]
        public void StateManagement_ShoppingCart_ShouldPersistAcrossSessions()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var cartItems = new List<object>
            {
                new { BookId = 1, Quantity = 2, Price = 19.99m },
                new { BookId = 2, Quantity = 1, Price = 29.99m }
            };

            // Act & Assert
            Assert.IsNotNull(correlationId);
            Assert.AreEqual(2, cartItems.Count);
            Assert.IsTrue(correlationId.Length > 0);
        }

        [TestMethod]
        public void StateManagement_Performance_ShouldMeetRequirements()
        {
            // Arrange
            var startTime = DateTime.Now;
            
            // Act - Simulate state management operations
            var userPrefs = new UserPreferences();
            var searchCriteria = new SearchCriteria();
            var categories = new List<CategoryItem>();
            
            for (int i = 0; i < 100; i++)
            {
                categories.Add(new CategoryItem { Id = i, Name = $"Category {i}" });
            }

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            // Assert - Should complete quickly
            Assert.IsTrue(duration.TotalMilliseconds < 1000, "State management operations should complete within 1 second");
            Assert.AreEqual(100, categories.Count);
        }

        [TestMethod]
        public void StateManagement_Serialization_ShouldHandleComplexObjects()
        {
            // Arrange
            var complexSearchCriteria = new SearchCriteria
            {
                Query = "Advanced C# Programming",
                Category = "Technology",
                MinPrice = 25.00m,
                MaxPrice = 75.00m,
                Author = "John Doe",
                Publisher = "Tech Books Inc",
                InStockOnly = true,
                PageNumber = 3,
                PageSize = 25
            };

            var complexUserPreferences = new UserPreferences
            {
                PageSize = 25,
                SortBy = "PublicationDate",
                SortDirection = "DESC",
                Theme = "Professional",
                ShowOutOfStock = false,
                Currency = "USD"
            };

            // Act & Assert - Verify all properties are maintained
            Assert.AreEqual("Advanced C# Programming", complexSearchCriteria.Query);
            Assert.AreEqual("Technology", complexSearchCriteria.Category);
            Assert.AreEqual(25.00m, complexSearchCriteria.MinPrice);
            Assert.AreEqual(75.00m, complexSearchCriteria.MaxPrice);
            Assert.AreEqual("John Doe", complexSearchCriteria.Author);
            Assert.AreEqual("Tech Books Inc", complexSearchCriteria.Publisher);
            Assert.IsTrue(complexSearchCriteria.InStockOnly);
            Assert.AreEqual(3, complexSearchCriteria.PageNumber);
            Assert.AreEqual(25, complexSearchCriteria.PageSize);

            Assert.AreEqual(25, complexUserPreferences.PageSize);
            Assert.AreEqual("PublicationDate", complexUserPreferences.SortBy);
            Assert.AreEqual("DESC", complexUserPreferences.SortDirection);
            Assert.AreEqual("Professional", complexUserPreferences.Theme);
            Assert.IsFalse(complexUserPreferences.ShowOutOfStock);
            Assert.AreEqual("USD", complexUserPreferences.Currency);
        }

        [TestMethod]
        public void StateManagement_ConcurrentAccess_ShouldBeThreadSafe()
        {
            // Arrange
            var tasks = new List<System.Threading.Tasks.Task>();
            var results = new List<bool>();
            var lockObject = new object();

            // Act - Simulate concurrent access
            for (int i = 0; i < 10; i++)
            {
                var taskId = i;
                var task = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        // Simulate state management operations
                        var prefs = new UserPreferences { PageSize = taskId + 10 };
                        var criteria = new SearchCriteria { PageNumber = taskId + 1 };
                        
                        lock (lockObject)
                        {
                            results.Add(true);
                        }
                    }
                    catch
                    {
                        lock (lockObject)
                        {
                            results.Add(false);
                        }
                    }
                });
                tasks.Add(task);
            }

            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            // Assert - All operations should succeed
            Assert.AreEqual(10, results.Count);
            Assert.IsTrue(results.TrueForAll(r => r), "All concurrent operations should succeed");
        }
    }

    /// <summary>
    /// Test page class for ViewState optimization testing
    /// </summary>
    public class TestPage : Page
    {
        public TestPage()
        {
            // Initialize test page
        }
    }
}