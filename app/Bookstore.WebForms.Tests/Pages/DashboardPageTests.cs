using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Bookstore.Domain.Books;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.WebForms.Admin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class DashboardPageTests
    {
        private Mock<IOrderService> mockOrderService;
        private Mock<IOfferService> mockOfferService;
        private Mock<IBookService> mockBookService;
        private Mock<HttpContextBase> mockHttpContext;
        private Mock<HttpRequestBase> mockHttpRequest;
        private Mock<HttpResponseBase> mockHttpResponse;
        private Dashboard dashboardPage;

        [TestInitialize]
        public void Setup()
        {
            mockOrderService = new Mock<IOrderService>();
            mockOfferService = new Mock<IOfferService>();
            mockBookService = new Mock<IBookService>();
            mockHttpContext = new Mock<HttpContextBase>();
            mockHttpRequest = new Mock<HttpRequestBase>();
            mockHttpResponse = new Mock<HttpResponseBase>();

            mockHttpContext.Setup(c => c.Request).Returns(mockHttpRequest.Object);
            mockHttpContext.Setup(c => c.Response).Returns(mockHttpResponse.Object);

            dashboardPage = new Dashboard();
            dashboardPage.OrderService = mockOrderService.Object;
            dashboardPage.OfferService = mockOfferService.Object;
            dashboardPage.BookService = mockBookService.Object;
        }

        [TestMethod]
        public async Task LoadDashboardDataAsync_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange
            var orderStats = new OrderStatistics
            {
                PendingOrders = 5,
                PastDueOrders = 2,
                OrdersThisMonth = 15,
                OrdersTotal = 100
            };

            var offerStats = new OfferStatistics
            {
                PendingOffers = 3,
                OffersThisMonth = 8,
                OffersTotal = 50
            };

            var inventoryStats = new BookStatistics
            {
                OutOfStock = 1,
                LowStock = 4,
                StockTotal = 200
            };

            mockOrderService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(orderStats);
            mockOfferService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(offerStats);
            mockBookService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(inventoryStats);

            // Act
            await InvokeLoadDashboardDataAsync();

            // Assert
            Assert.AreEqual(5, dashboardPage.PendingOrders);
            Assert.AreEqual(2, dashboardPage.PastDueOrders);
            Assert.AreEqual(15, dashboardPage.OrdersThisMonth);
            Assert.AreEqual(100, dashboardPage.OrdersTotal);
            Assert.AreEqual(3, dashboardPage.PendingOffers);
            Assert.AreEqual(8, dashboardPage.OffersThisMonth);
            Assert.AreEqual(50, dashboardPage.OffersTotal);
            Assert.AreEqual(1, dashboardPage.OutOfStock);
            Assert.AreEqual(4, dashboardPage.LowStock);
            Assert.AreEqual(200, dashboardPage.StockTotal);
        }

        [TestMethod]
        public async Task LoadDashboardDataAsync_WithServiceException_HandlesErrorGracefully()
        {
            // Arrange
            mockOrderService.Setup(s => s.GetStatisticsAsync()).ThrowsAsync(new Exception("Service error"));

            // Act & Assert - Should not throw exception
            try
            {
                await InvokeLoadDashboardDataAsync();
                // Test passes if no exception is thrown
            }
            catch (Exception)
            {
                Assert.Fail("LoadDashboardDataAsync should handle exceptions gracefully");
            }
        }

        [TestMethod]
        public void BindDashboardData_WithSingleValues_DisplaysSingularText()
        {
            // Arrange
            SetupDashboardPageWithSingleValues();
            var mockControls = SetupMockControls();

            // Act
            InvokeBindDashboardData();

            // Assert
            Assert.IsTrue(mockControls.lblPendingOrders.Text.Contains("There is 1 pending order"));
            Assert.IsTrue(mockControls.lblPastDueOrders.Text.Contains("There is 1 past-due order"));
            Assert.IsTrue(mockControls.lblOrdersThisMonth.Text.Contains("There has been 1 order placed this month"));
            Assert.IsTrue(mockControls.lblOrdersTotal.Text.Contains("There has been 1 order placed in total"));
            Assert.IsTrue(mockControls.lblPendingOffers.Text.Contains("There is 1 pending offer"));
            Assert.IsTrue(mockControls.lblOffersThisMonth.Text.Contains("There has been 1 offer made this month"));
            Assert.IsTrue(mockControls.lblOffersTotal.Text.Contains("There has been 1 offer made in total"));
            Assert.IsTrue(mockControls.lblOutOfStock.Text.Contains("1 book is out of stock"));
            Assert.IsTrue(mockControls.lblLowStock.Text.Contains("1 book is low in stock"));
            Assert.IsTrue(mockControls.lblStockTotal.Text.Contains("There is a total of 1 book in inventory"));
        }

        [TestMethod]
        public void BindDashboardData_WithMultipleValues_DisplaysPluralText()
        {
            // Arrange
            SetupDashboardPageWithMultipleValues();
            var mockControls = SetupMockControls();

            // Act
            InvokeBindDashboardData();

            // Assert
            Assert.IsTrue(mockControls.lblPendingOrders.Text.Contains("There are 5 pending orders"));
            Assert.IsTrue(mockControls.lblPastDueOrders.Text.Contains("There are 2 past-due orders"));
            Assert.IsTrue(mockControls.lblOrdersThisMonth.Text.Contains("There have been 15 orders placed this month"));
            Assert.IsTrue(mockControls.lblOrdersTotal.Text.Contains("There have been 100 orders placed in total"));
            Assert.IsTrue(mockControls.lblPendingOffers.Text.Contains("There are 3 pending offers"));
            Assert.IsTrue(mockControls.lblOffersThisMonth.Text.Contains("There have been 8 offers made this month"));
            Assert.IsTrue(mockControls.lblOffersTotal.Text.Contains("There have been 50 offers made in total"));
            Assert.IsTrue(mockControls.lblOutOfStock.Text.Contains("4 books are out of stock"));
            Assert.IsTrue(mockControls.lblLowStock.Text.Contains("6 books are low in stock"));
            Assert.IsTrue(mockControls.lblStockTotal.Text.Contains("There is a total of 200 books in inventory"));
        }

        [TestMethod]
        public void BindDashboardData_SetsCorrectNavigationUrls()
        {
            // Arrange
            SetupDashboardPageWithMultipleValues();
            var mockControls = SetupMockControls();

            // Act
            InvokeBindDashboardData();

            // Assert
            Assert.AreEqual($"~/Admin/Orders.aspx?status={(int)OrderStatus.Pending}", mockControls.lnkPendingOrders.NavigateUrl);
            Assert.AreEqual("~/Admin/Orders.aspx", mockControls.lnkPastDueOrders.NavigateUrl);
            Assert.IsTrue(mockControls.lnkOrdersThisMonth.NavigateUrl.StartsWith("~/Admin/Orders.aspx?dateFrom="));
            Assert.AreEqual("~/Admin/Orders.aspx", mockControls.lnkOrdersTotal.NavigateUrl);
            
            Assert.AreEqual($"~/Admin/Offers.aspx?status={(int)OfferStatus.PendingApproval}", mockControls.lnkPendingOffers.NavigateUrl);
            Assert.IsTrue(mockControls.lnkOffersThisMonth.NavigateUrl.StartsWith("~/Admin/Offers.aspx?dateFrom="));
            Assert.AreEqual("~/Admin/Offers.aspx", mockControls.lnkOffersTotal.NavigateUrl);
            
            Assert.AreEqual("~/Admin/Inventory.aspx?lowStock=true", mockControls.lnkOutOfStock.NavigateUrl);
            Assert.AreEqual("~/Admin/Inventory.aspx?lowStock=true", mockControls.lnkLowStock.NavigateUrl);
            Assert.AreEqual("~/Admin/Inventory.aspx", mockControls.lnkStockTotal.NavigateUrl);
        }

        [TestMethod]
        public void ShowError_DisplaysErrorMessage()
        {
            // Arrange
            var mockControls = SetupMockControls();
            const string errorMessage = "Test error message";

            // Act
            InvokeShowError(errorMessage);

            // Assert
            Assert.AreEqual(errorMessage, mockControls.lblError.Text);
            Assert.IsTrue(mockControls.pnlError.Visible);
        }

        [TestMethod]
        public void ShowLoadingPanel_ShowsAndHidesCorrectly()
        {
            // Arrange
            var mockControls = SetupMockControls();

            // Act & Assert - Show loading
            InvokeShowLoadingPanel(true);
            Assert.IsTrue(mockControls.pnlLoading.Visible);

            // Act & Assert - Hide loading
            InvokeShowLoadingPanel(false);
            Assert.IsFalse(mockControls.pnlLoading.Visible);
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

        private void InvokeShowError(string message)
        {
            var method = typeof(Dashboard).GetMethod("ShowError", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(dashboardPage, new object[] { message });
        }

        private void InvokeShowLoadingPanel(bool show)
        {
            var method = typeof(Dashboard).GetMethod("ShowLoadingPanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(dashboardPage, new object[] { show });
        }

        private void SetupDashboardPageWithSingleValues()
        {
            var orderStatsProperty = typeof(Dashboard).GetProperty("PendingOrders");
            orderStatsProperty.SetValue(dashboardPage, 1);
            
            typeof(Dashboard).GetProperty("PastDueOrders").SetValue(dashboardPage, 1);
            typeof(Dashboard).GetProperty("OrdersThisMonth").SetValue(dashboardPage, 1);
            typeof(Dashboard).GetProperty("OrdersTotal").SetValue(dashboardPage, 1);
            typeof(Dashboard).GetProperty("PendingOffers").SetValue(dashboardPage, 1);
            typeof(Dashboard).GetProperty("OffersThisMonth").SetValue(dashboardPage, 1);
            typeof(Dashboard).GetProperty("OffersTotal").SetValue(dashboardPage, 1);
            typeof(Dashboard).GetProperty("OutOfStock").SetValue(dashboardPage, 1);
            typeof(Dashboard).GetProperty("LowStock").SetValue(dashboardPage, 1);
            typeof(Dashboard).GetProperty("StockTotal").SetValue(dashboardPage, 1);
        }

        private void SetupDashboardPageWithMultipleValues()
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
                lblPendingOrders = new MockLabel(),
                lnkPendingOrders = new MockHyperLink(),
                lblPastDueOrders = new MockLabel(),
                lnkPastDueOrders = new MockHyperLink(),
                lblOrdersThisMonth = new MockLabel(),
                lnkOrdersThisMonth = new MockHyperLink(),
                lblOrdersTotal = new MockLabel(),
                lnkOrdersTotal = new MockHyperLink(),
                lblPendingOffers = new MockLabel(),
                lnkPendingOffers = new MockHyperLink(),
                lblOffersThisMonth = new MockLabel(),
                lnkOffersThisMonth = new MockHyperLink(),
                lblOffersTotal = new MockLabel(),
                lnkOffersTotal = new MockHyperLink(),
                lblOutOfStock = new MockLabel(),
                lnkOutOfStock = new MockHyperLink(),
                lblLowStock = new MockLabel(),
                lnkLowStock = new MockHyperLink(),
                lblStockTotal = new MockLabel(),
                lnkStockTotal = new MockHyperLink(),
                pnlLoading = new MockPanel(),
                pnlError = new MockPanel(),
                lblError = new MockLabel()
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

        #endregion

        #region Mock Control Classes

        private class MockLabel
        {
            public string Text { get; set; }
        }

        private class MockHyperLink
        {
            public string NavigateUrl { get; set; }
            public string Text { get; set; }
        }

        private class MockPanel
        {
            public bool Visible { get; set; }
        }

        #endregion
    }
}