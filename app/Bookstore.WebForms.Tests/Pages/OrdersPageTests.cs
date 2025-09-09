using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Bookstore.Domain.Orders;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Web;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class OrdersPageTests
    {
        private Mock<IOrderService> _mockOrderService;
        private Orders _ordersPage;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;
        private Mock<HttpServerUtilityBase> _mockServer;

        [TestInitialize]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _ordersPage = new Orders();
            _ordersPage.OrderService = _mockOrderService.Object;

            // Setup mock HTTP context
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockRequest = new Mock<HttpRequestBase>();
            _mockResponse = new Mock<HttpResponseBase>();
            _mockServer = new Mock<HttpServerUtilityBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
            _mockHttpContext.Setup(c => c.Server).Returns(_mockServer.Object);
            _mockServer.Setup(s => s.UrlEncode(It.IsAny<string>())).Returns<string>(s => s);

            // Setup authenticated user
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-123")
            }, "test");
            var principal = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(principal);
        }

        [TestMethod]
        public async Task LoadOrdersAsync_WithValidUser_LoadsOrdersSuccessfully()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, SubTotal = 25.99m, DeliveryDate = DateTime.Now.AddDays(3), OrderStatus = OrderStatus.Processing },
                new Order { Id = 2, SubTotal = 45.50m, DeliveryDate = DateTime.Now.AddDays(5), OrderStatus = OrderStatus.Shipped }
            };

            _mockOrderService.Setup(s => s.GetOrdersAsync("test-user-123"))
                .ReturnsAsync(orders);

            // Act
            await _ordersPage.LoadOrdersAsync();

            // Assert
            _mockOrderService.Verify(s => s.GetOrdersAsync("test-user-123"), Times.Once);
            Assert.IsFalse(_ordersPage.NoOrdersPanel.Visible);
            Assert.IsTrue(_ordersPage.OrdersPanel.Visible);
        }

        [TestMethod]
        public async Task LoadOrdersAsync_WithNoOrders_ShowsNoOrdersMessage()
        {
            // Arrange
            var orders = new List<Order>();

            _mockOrderService.Setup(s => s.GetOrdersAsync("test-user-123"))
                .ReturnsAsync(orders);

            // Act
            await _ordersPage.LoadOrdersAsync();

            // Assert
            _mockOrderService.Verify(s => s.GetOrdersAsync("test-user-123"), Times.Once);
            Assert.IsTrue(_ordersPage.NoOrdersPanel.Visible);
            Assert.IsFalse(_ordersPage.OrdersPanel.Visible);
        }

        [TestMethod]
        public async Task CancelOrderAsync_WithValidOrder_CancelsOrderSuccessfully()
        {
            // Arrange
            int orderId = 123;
            _mockOrderService.Setup(s => s.CancelOrderAsync(It.IsAny<CancelOrderDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _ordersPage.CancelOrderAsync(orderId);

            // Assert
            _mockOrderService.Verify(s => s.CancelOrderAsync(It.Is<CancelOrderDto>(dto => 
                dto.UserId == "test-user-123" && dto.OrderId == orderId)), Times.Once);
        }

        [TestMethod]
        public async Task CancelOrderAsync_WithServiceException_HandlesErrorGracefully()
        {
            // Arrange
            int orderId = 123;
            _mockOrderService.Setup(s => s.CancelOrderAsync(It.IsAny<CancelOrderDto>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            await _ordersPage.CancelOrderAsync(orderId);

            // Assert
            _mockOrderService.Verify(s => s.CancelOrderAsync(It.IsAny<CancelOrderDto>()), Times.Once);
            // Error should be handled gracefully
        }
    }
}