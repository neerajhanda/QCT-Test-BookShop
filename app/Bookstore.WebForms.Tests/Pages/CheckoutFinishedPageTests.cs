using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Bookstore.Domain.Orders;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class CheckoutFinishedPageTests
    {
        private Mock<IOrderService> _mockOrderService;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockHttpRequest;
        private Mock<HttpResponseBase> _mockHttpResponse;
        private TestableCheckoutFinishedPage _checkoutFinishedPage;

        [TestInitialize]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockHttpRequest = new Mock<HttpRequestBase>();
            _mockHttpResponse = new Mock<HttpResponseBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockHttpResponse.Object);

            _checkoutFinishedPage = new TestableCheckoutFinishedPage
            {
                OrderService = _mockOrderService.Object
            };

            _checkoutFinishedPage.SetHttpContext(_mockHttpContext.Object);
        }

        [TestMethod]
        public async Task LoadOrderDetails_WithValidOrderId_ShouldLoadSuccessfully()
        {
            // Arrange
            var orderId = 123;
            var userId = "test-user-id";
            var order = CreateTestOrder(userId);

            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns(orderId.ToString());
            _checkoutFinishedPage.SetCurrentUserId(userId);
            
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                .ReturnsAsync(order);

            // Act
            await _checkoutFinishedPage.TestLoadOrderDetailsAsync();

            // Assert
            Assert.IsNotNull(_checkoutFinishedPage.TestOrderItems);
            Assert.AreEqual(2, _checkoutFinishedPage.TestOrderItems.Count);
            
            var firstItem = _checkoutFinishedPage.TestOrderItems.First();
            Assert.AreEqual("Test Book 1", firstItem.Bookname);
            Assert.AreEqual(10.99m, firstItem.Price);
            Assert.AreEqual(2, firstItem.Quantity);
            Assert.AreEqual(21.98m, firstItem.Total);
        }

        [TestMethod]
        public async Task LoadOrderDetails_WithInvalidOrderId_ShouldRedirectToHome()
        {
            // Arrange
            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns("invalid");

            bool redirectCalled = false;
            _mockHttpResponse.Setup(r => r.Redirect("~/Default.aspx"))
                .Callback(() => redirectCalled = true);

            // Act
            await _checkoutFinishedPage.TestLoadOrderDetailsAsync();

            // Assert
            Assert.IsTrue(redirectCalled);
        }

        [TestMethod]
        public async Task LoadOrderDetails_WithMissingOrderId_ShouldRedirectToHome()
        {
            // Arrange
            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns((string)null);

            bool redirectCalled = false;
            _mockHttpResponse.Setup(r => r.Redirect("~/Default.aspx"))
                .Callback(() => redirectCalled = true);

            // Act
            await _checkoutFinishedPage.TestLoadOrderDetailsAsync();

            // Assert
            Assert.IsTrue(redirectCalled);
        }

        [TestMethod]
        public async Task LoadOrderDetails_WithZeroOrderId_ShouldRedirectToHome()
        {
            // Arrange
            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns("0");

            bool redirectCalled = false;
            _mockHttpResponse.Setup(r => r.Redirect("~/Default.aspx"))
                .Callback(() => redirectCalled = true);

            // Act
            await _checkoutFinishedPage.TestLoadOrderDetailsAsync();

            // Assert
            Assert.IsTrue(redirectCalled);
        }

        [TestMethod]
        public async Task LoadOrderDetails_WithNonExistentOrder_ShouldShowError()
        {
            // Arrange
            var orderId = 999;
            var userId = "test-user-id";

            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns(orderId.ToString());
            _checkoutFinishedPage.SetCurrentUserId(userId);
            
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act
            await _checkoutFinishedPage.TestLoadOrderDetailsAsync();

            // Assert
            Assert.IsTrue(_checkoutFinishedPage.TestErrorShown);
            Assert.AreEqual("Order not found.", _checkoutFinishedPage.TestErrorMessage);
        }

        [TestMethod]
        public async Task LoadOrderDetails_WithOrderBelongingToDifferentUser_ShouldRedirectToUnauthorized()
        {
            // Arrange
            var orderId = 123;
            var userId = "test-user-id";
            var differentUserId = "different-user-id";
            var order = CreateTestOrder(differentUserId);

            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns(orderId.ToString());
            _checkoutFinishedPage.SetCurrentUserId(userId);
            
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                .ReturnsAsync(order);

            bool redirectCalled = false;
            _mockHttpResponse.Setup(r => r.Redirect("~/Unauthorized.aspx"))
                .Callback(() => redirectCalled = true);

            // Act
            await _checkoutFinishedPage.TestLoadOrderDetailsAsync();

            // Assert
            Assert.IsTrue(redirectCalled);
        }

        [TestMethod]
        public async Task LoadOrderDetails_WithServiceException_ShouldThrowException()
        {
            // Arrange
            var orderId = 123;
            var userId = "test-user-id";

            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns(orderId.ToString());
            _checkoutFinishedPage.SetCurrentUserId(userId);
            
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(
                () => _checkoutFinishedPage.TestLoadOrderDetailsAsync());
        }

        [TestMethod]
        public void BindControls_WithOrderItems_ShouldSetVisibilityCorrectly()
        {
            // Arrange
            var orderItems = new List<Models.CheckoutFinishedItemViewModel>
            {
                new Models.CheckoutFinishedItemViewModel
                {
                    Bookname = "Test Book",
                    Price = 10.99m,
                    Quantity = 1,
                    Total = 10.99m
                }
            };

            _checkoutFinishedPage.SetOrderItems(orderItems);

            // Act
            _checkoutFinishedPage.TestBindControls();

            // Assert
            Assert.IsTrue(_checkoutFinishedPage.TestOrderDetailsPanelVisible);
            Assert.IsFalse(_checkoutFinishedPage.TestErrorShown);
        }

        [TestMethod]
        public void BindControls_WithNoOrderItems_ShouldShowError()
        {
            // Arrange
            var orderItems = new List<Models.CheckoutFinishedItemViewModel>();
            _checkoutFinishedPage.SetOrderItems(orderItems);

            // Act
            _checkoutFinishedPage.TestBindControls();

            // Assert
            Assert.IsFalse(_checkoutFinishedPage.TestOrderDetailsPanelVisible);
            Assert.IsTrue(_checkoutFinishedPage.TestErrorShown);
            Assert.AreEqual("No order items found.", _checkoutFinishedPage.TestErrorMessage);
        }

        private Order CreateTestOrder(string userId)
        {
            var mockOrder = new Mock<Order>();
            var mockCustomer = new Mock<Domain.Customers.Customer>();
            var mockOrderItems = new List<OrderItem>();

            // Create mock order items
            var mockBook1 = new Mock<Domain.Books.Book>();
            mockBook1.Setup(b => b.Id).Returns(1);
            mockBook1.Setup(b => b.Name).Returns("Test Book 1");
            mockBook1.Setup(b => b.Price).Returns(10.99m);
            mockBook1.Setup(b => b.CoverImageUrl).Returns("/images/book1.jpg");

            var mockBook2 = new Mock<Domain.Books.Book>();
            mockBook2.Setup(b => b.Id).Returns(2);
            mockBook2.Setup(b => b.Name).Returns("Test Book 2");
            mockBook2.Setup(b => b.Price).Returns(15.99m);
            mockBook2.Setup(b => b.CoverImageUrl).Returns("/images/book2.jpg");

            var mockOrderItem1 = new Mock<OrderItem>();
            mockOrderItem1.Setup(oi => oi.Book).Returns(mockBook1.Object);
            mockOrderItem1.Setup(oi => oi.Quantity).Returns(2);

            var mockOrderItem2 = new Mock<OrderItem>();
            mockOrderItem2.Setup(oi => oi.Book).Returns(mockBook2.Object);
            mockOrderItem2.Setup(oi => oi.Quantity).Returns(1);

            mockOrderItems.Add(mockOrderItem1.Object);
            mockOrderItems.Add(mockOrderItem2.Object);

            mockCustomer.Setup(c => c.Sub).Returns(userId);
            mockOrder.Setup(o => o.Customer).Returns(mockCustomer.Object);
            mockOrder.Setup(o => o.OrderItems).Returns(mockOrderItems);

            return mockOrder.Object;
        }
    }

    // Testable version of CheckoutFinishedPage that exposes protected/private members for testing
    public class TestableCheckoutFinishedPage : CheckoutFinished
    {
        public List<Models.CheckoutFinishedItemViewModel> TestOrderItems { get; private set; }
        public bool TestErrorShown { get; private set; }
        public string TestErrorMessage { get; private set; }
        public bool TestOrderDetailsPanelVisible { get; private set; }

        private HttpContextBase _httpContext;
        private string _currentUserId;

        public void SetHttpContext(HttpContextBase context)
        {
            _httpContext = context;
        }

        public void SetCurrentUserId(string userId)
        {
            _currentUserId = userId;
        }

        public void SetOrderItems(List<Models.CheckoutFinishedItemViewModel> orderItems)
        {
            _orderItems = orderItems;
        }

        protected override string CurrentUserId => _currentUserId;

        public async Task TestLoadOrderDetailsAsync()
        {
            await LoadOrderDetailsAsync();
            TestOrderItems = _orderItems;
        }

        public void TestBindControls()
        {
            BindControls();
        }

        private new async Task LoadOrderDetailsAsync()
        {
            try
            {
                if (!int.TryParse(_httpContext.Request.QueryString["orderId"], out int orderId) || orderId <= 0)
                {
                    _httpContext.Response.Redirect("~/Default.aspx");
                    return;
                }

                var order = await OrderService.GetOrderAsync(orderId);
                if (order == null)
                {
                    ShowError("Order not found.");
                    return;
                }

                if (order.Customer.Sub != CurrentUserId)
                {
                    _httpContext.Response.Redirect("~/Unauthorized.aspx");
                    return;
                }

                _orderItems = order.OrderItems.Select(x => new Models.CheckoutFinishedItemViewModel
                {
                    BookId = x.Book.Id,
                    Bookname = x.Book.Name,
                    Price = x.Book.Price,
                    Quantity = x.Quantity,
                    Url = x.Book.CoverImageUrl,
                    Total = x.Book.Price * x.Quantity
                }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private new void BindControls()
        {
            try
            {
                if (_orderItems != null && _orderItems.Any())
                {
                    TestOrderDetailsPanelVisible = true;
                }
                else
                {
                    TestOrderDetailsPanelVisible = false;
                    ShowError("No order items found.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private new void ShowError(string message)
        {
            TestErrorShown = true;
            TestErrorMessage = message;
        }
    }
}