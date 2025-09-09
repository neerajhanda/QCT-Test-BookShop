using System;
using System.Threading.Tasks;
using System.Web;
using Bookstore.Domain.Books;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.WebForms.Admin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class DashboardIntegrationTests
    {
        private Mock<IOrderService> mockOrderService;
        private Mock<IOfferService> mockOfferService;
        private Mock<IBookService> mockBookService;
        private Dashboard dashboardPage;

        [TestInitialize]
        public void Setup()
        {
            mockOrderService = new Mock<IOrderService>();
            mockOfferService = new Mock<IOfferService>();
            mockBookService = new Mock<IBookService>();

            dashboardPage = new Dashboard();
            dashboardPage.OrderService = mockOrderService.Object;
            dashboardPage.OfferService = mockOfferService.Object;
            dashboardPage.BookService = mockBookService.Object;
        }

        [TestMethod]
        public async Task Dashboard_LoadsAllStatistics_Successfully()
        {
            // Arrange
            var orderStats = new OrderStatistics
            {
                PendingOrders = 10,
                PastDueOrders = 3,
                OrdersThisMonth = 25,
                OrdersTotal = 150
            };

            var offerStats = new OfferStatistics
            {
                PendingOffers = 5,
                OffersThisMonth = 12,
                OffersTotal = 75
            };

            var inventoryStats = new BookStatistics
            {
                OutOfStock = 2,
                LowStock = 8,
                StockTotal = 300
            };

            mockOrderService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(orderStats);
            mockOfferService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(offerStats);
            mockBookService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(inventoryStats);

            // Act
            await InvokeLoadDashboardDataAsync();

            // Assert
            mockOrderService.Verify(s => s.GetStatisticsAsync(), Times.Once);
            mockOfferService.Verify(s => s.GetStatisticsAsync(), Times.Once);
            mockBookService.Verify(s => s.GetStatisticsAsync(), Times.Once);

            Assert.AreEqual(10, dashboardPage.PendingOrders);
            Assert.AreEqual(3, dashboardPage.PastDueOrders);
            Assert.AreEqual(25, dashboardPage.OrdersThisMonth);
            Assert.AreEqual(150, dashboardPage.OrdersTotal);
            Assert.AreEqual(5, dashboardPage.PendingOffers);
            Assert.AreEqual(12, dashboardPage.OffersThisMonth);
            Assert.AreEqual(75, dashboardPage.OffersTotal);
            Assert.AreEqual(2, dashboardPage.OutOfStock);
            Assert.AreEqual(8, dashboardPage.LowStock);
            Assert.AreEqual(300, dashboardPage.StockTotal);
        }

        [TestMethod]
        public async Task Dashboard_WithZeroValues_DisplaysCorrectly()
        {
            // Arrange
            var orderStats = new OrderStatistics
            {
                PendingOrders = 0,
                PastDueOrders = 0,
                OrdersThisMonth = 0,
                OrdersTotal = 0
            };

            var offerStats = new OfferStatistics
            {
                PendingOffers = 0,
                OffersThisMonth = 0,
                OffersTotal = 0
            };

            var inventoryStats = new BookStatistics
            {
                OutOfStock = 0,
                LowStock = 0,
                StockTotal = 0
            };

            mockOrderService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(orderStats);
            mockOfferService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(offerStats);
            mockBookService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(inventoryStats);

            // Act
            await InvokeLoadDashboardDataAsync();

            // Assert - All values should be zero
            Assert.AreEqual(0, dashboardPage.PendingOrders);
            Assert.AreEqual(0, dashboardPage.PastDueOrders);
            Assert.AreEqual(0, dashboardPage.OrdersThisMonth);
            Assert.AreEqual(0, dashboardPage.OrdersTotal);
            Assert.AreEqual(0, dashboardPage.PendingOffers);
            Assert.AreEqual(0, dashboardPage.OffersThisMonth);
            Assert.AreEqual(0, dashboardPage.OffersTotal);
            Assert.AreEqual(0, dashboardPage.OutOfStock);
            Assert.AreEqual(0, dashboardPage.LowStock);
            Assert.AreEqual(0, dashboardPage.StockTotal);
        }

        [TestMethod]
        public async Task Dashboard_WithOrderServiceFailure_HandlesGracefully()
        {
            // Arrange
            mockOrderService.Setup(s => s.GetStatisticsAsync())
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            var offerStats = new OfferStatistics { PendingOffers = 5 };
            var inventoryStats = new BookStatistics { StockTotal = 100 };

            mockOfferService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(offerStats);
            mockBookService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(inventoryStats);

            // Act & Assert - Should handle exception gracefully
            try
            {
                await InvokeLoadDashboardDataAsync();
                // Test passes if no exception is thrown
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dashboard should handle service failures gracefully. Exception: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task Dashboard_WithOfferServiceFailure_HandlesGracefully()
        {
            // Arrange
            var orderStats = new OrderStatistics { PendingOrders = 10 };
            var inventoryStats = new BookStatistics { StockTotal = 100 };

            mockOrderService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(orderStats);
            mockOfferService.Setup(s => s.GetStatisticsAsync())
                .ThrowsAsync(new TimeoutException("Service timeout"));
            mockBookService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(inventoryStats);

            // Act & Assert - Should handle exception gracefully
            try
            {
                await InvokeLoadDashboardDataAsync();
                // Test passes if no exception is thrown
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dashboard should handle service failures gracefully. Exception: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task Dashboard_WithInventoryServiceFailure_HandlesGracefully()
        {
            // Arrange
            var orderStats = new OrderStatistics { PendingOrders = 10 };
            var offerStats = new OfferStatistics { PendingOffers = 5 };

            mockOrderService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(orderStats);
            mockOfferService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(offerStats);
            mockBookService.Setup(s => s.GetStatisticsAsync())
                .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

            // Act & Assert - Should handle exception gracefully
            try
            {
                await InvokeLoadDashboardDataAsync();
                // Test passes if no exception is thrown
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dashboard should handle service failures gracefully. Exception: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task Dashboard_WithAllServicesFailure_HandlesGracefully()
        {
            // Arrange
            mockOrderService.Setup(s => s.GetStatisticsAsync())
                .ThrowsAsync(new Exception("Order service failed"));
            mockOfferService.Setup(s => s.GetStatisticsAsync())
                .ThrowsAsync(new Exception("Offer service failed"));
            mockBookService.Setup(s => s.GetStatisticsAsync())
                .ThrowsAsync(new Exception("Book service failed"));

            // Act & Assert - Should handle exception gracefully
            try
            {
                await InvokeLoadDashboardDataAsync();
                // Test passes if no exception is thrown
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dashboard should handle all service failures gracefully. Exception: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task Dashboard_ConcurrentServiceCalls_CompletesSuccessfully()
        {
            // Arrange
            var orderStats = new OrderStatistics { PendingOrders = 15 };
            var offerStats = new OfferStatistics { PendingOffers = 8 };
            var inventoryStats = new BookStatistics { StockTotal = 250 };

            // Add delays to simulate real service calls
            mockOrderService.Setup(s => s.GetStatisticsAsync())
                .Returns(Task.Delay(100).ContinueWith(_ => orderStats));
            mockOfferService.Setup(s => s.GetStatisticsAsync())
                .Returns(Task.Delay(150).ContinueWith(_ => offerStats));
            mockBookService.Setup(s => s.GetStatisticsAsync())
                .Returns(Task.Delay(200).ContinueWith(_ => inventoryStats));

            var startTime = DateTime.UtcNow;

            // Act
            await InvokeLoadDashboardDataAsync();

            var endTime = DateTime.UtcNow;
            var totalTime = endTime - startTime;

            // Assert - Should complete in less time than sequential calls would take
            Assert.IsTrue(totalTime.TotalMilliseconds < 400, 
                "Concurrent calls should complete faster than sequential calls");
            
            Assert.AreEqual(15, dashboardPage.PendingOrders);
            Assert.AreEqual(8, dashboardPage.PendingOffers);
            Assert.AreEqual(250, dashboardPage.StockTotal);
        }

        [TestMethod]
        public void Dashboard_NavigationUrls_AreCorrectlyFormatted()
        {
            // Arrange
            SetupDashboardWithTestData();
            var mockControls = SetupMockControls();

            // Act
            InvokeBindDashboardData();

            // Assert - Check URL formats
            Assert.IsTrue(mockControls.lnkPendingOrders.NavigateUrl.Contains("~/Admin/Orders.aspx"));
            Assert.IsTrue(mockControls.lnkPendingOffers.NavigateUrl.Contains("~/Admin/Offers.aspx"));
            Assert.IsTrue(mockControls.lnkOutOfStock.NavigateUrl.Contains("~/Admin/Inventory.aspx"));
            
            // Check date formatting in URLs
            Assert.IsTrue(mockControls.lnkOrdersThisMonth.NavigateUrl.Contains("dateFrom="));
            Assert.IsTrue(mockControls.lnkOffersThisMonth.NavigateUrl.Contains("dateFrom="));
            
            // Check query parameters
            Assert.IsTrue(mockControls.lnkPendingOrders.NavigateUrl.Contains("status="));
            Assert.IsTrue(mockControls.lnkLowStock.NavigateUrl.Contains("lowStock=true"));
        }

        #region Helper Methods

        private async Task InvokeLoadDashboardDataAsync()
        {
            var method = typeof(Dashboard).GetMethod("LoadDashboardDataAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(dashboardPage, null);
        }

        private void InvokeBindDashboardData()
        {
            var method = typeof(Dashboard).GetMethod("BindDashboardData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(dashboardPage, null);
        }

        private void SetupDashboardWithTestData()
        {
            typeof(Dashboard).GetProperty("PendingOrders").SetValue(dashboardPage, 5);
            typeof(Dashboard).GetProperty("PastDueOrders").SetValue(dashboardPage, 2);
            typeof(Dashboard).GetProperty("OrdersThisMonth").SetValue(dashboardPage, 15);
            typeof(Dashboard).GetProperty("OrdersTotal").SetValue(dashboardPage, 100);
            typeof(Dashboard).GetProperty("PendingOffers").SetValue(dashboardPage, 3);
            typeof(Dashboard).GetProperty("OffersThisMonth").SetValue(dashboardPage, 8);
            typeof(Dashboard).GetProperty("OffersTotal").SetValue(dashboardPage, 50);
            typeof(Dashboard).GetProperty("OutOfStock").SetValue(dashboardPage, 4);
            typeof(Dashboard).GetProperty("LowStock").SetValue(dashboardPage, 6);
            typeof(Dashboard).GetProperty("StockTotal").SetValue(dashboardPage, 200);
        }

        private dynamic SetupMockControls()
        {
            var mockControls = new
            {
                lnkPendingOrders = new MockHyperLink(),
                lnkPendingOffers = new MockHyperLink(),
                lnkOutOfStock = new MockHyperLink(),
                lnkOrdersThisMonth = new MockHyperLink(),
                lnkOffersThisMonth = new MockHyperLink(),
                lnkLowStock = new MockHyperLink()
            };

            // Use reflection to set the mock controls on the page
            foreach (var property in mockControls.GetType().GetProperties())
            {
                var field = typeof(Dashboard).GetField(property.Name, 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(dashboardPage, property.GetValue(mockControls));
            }

            return mockControls;
        }

        private class MockHyperLink
        {
            public string NavigateUrl { get; set; }
            public string Text { get; set; }
        }

        #endregion
    }
}