using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Orders;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class CheckoutIntegrationTests
    {
        private Mock<IAddressService> _mockAddressService;
        private Mock<IShoppingCartService> _mockShoppingCartService;
        private Mock<IOrderService> _mockOrderService;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockHttpRequest;
        private Mock<HttpResponseBase> _mockHttpResponse;
        private Mock<HttpSessionStateBase> _mockSession;

        [TestInitialize]
        public void Setup()
        {
            _mockAddressService = new Mock<IAddressService>();
            _mockShoppingCartService = new Mock<IShoppingCartService>();
            _mockOrderService = new Mock<IOrderService>();
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockHttpRequest = new Mock<HttpRequestBase>();
            _mockHttpResponse = new Mock<HttpResponseBase>();
            _mockSession = new Mock<HttpSessionStateBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockHttpResponse.Object);
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }

        [TestMethod]
        public async Task CompleteCheckoutWorkflow_WithValidData_ShouldSucceed()
        {
            // Arrange
            var userId = "test-user-123";
            var correlationId = "cart-correlation-123";
            var addressId = 1;
            var orderId = 456;

            var addresses = CreateTestAddresses();
            var shoppingCart = CreateTestShoppingCart();
            var order = CreateTestOrder(userId, orderId);

            // Setup mocks
            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _mockAddressService.Setup(s => s.GetAddressesAsync(userId)).ReturnsAsync(addresses);
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync(correlationId)).ReturnsAsync(shoppingCart);
            _mockOrderService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDto>())).ReturnsAsync(orderId);
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId)).ReturnsAsync(order);

            // Test Checkout Page Load
            var checkoutPage = new TestableCheckoutPage
            {
                AddressService = _mockAddressService.Object,
                ShoppingCartService = _mockShoppingCartService.Object,
                OrderService = _mockOrderService.Object
            };
            checkoutPage.SetHttpContext(_mockHttpContext.Object);
            checkoutPage.SetCurrentUserId(userId);

            await checkoutPage.TestLoadCheckoutDataAsync();

            // Verify checkout page loaded correctly
            Assert.IsNotNull(checkoutPage.TestAddresses);
            Assert.AreEqual(2, checkoutPage.TestAddresses.Count);
            Assert.IsNotNull(checkoutPage.TestShoppingCartItems);
            Assert.AreEqual(2, checkoutPage.TestShoppingCartItems.Count);
            Assert.AreEqual(26.98m, checkoutPage.TestTotal);

            // Test Order Creation
            string redirectUrl = null;
            _mockHttpResponse.Setup(r => r.Redirect(It.IsAny<string>()))
                .Callback<string>(url => redirectUrl = url);

            await checkoutPage.TestFinishOrder(addressId);

            // Verify order was created and redirect occurred
            Assert.AreEqual($"~/CheckoutFinished.aspx?orderId={orderId}", redirectUrl);
            _mockOrderService.Verify(s => s.CreateOrderAsync(It.Is<CreateOrderDto>(dto =>
                dto.CustomerSub == userId &&
                dto.CorrelationId == correlationId &&
                dto.AddressId == addressId)), Times.Once);

            // Test Checkout Finished Page Load
            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns(orderId.ToString());

            var checkoutFinishedPage = new TestableCheckoutFinishedPage
            {
                OrderService = _mockOrderService.Object
            };
            checkoutFinishedPage.SetHttpContext(_mockHttpContext.Object);
            checkoutFinishedPage.SetCurrentUserId(userId);

            await checkoutFinishedPage.TestLoadOrderDetailsAsync();

            // Verify checkout finished page loaded correctly
            Assert.IsNotNull(checkoutFinishedPage.TestOrderItems);
            Assert.AreEqual(2, checkoutFinishedPage.TestOrderItems.Count);
            
            var firstItem = checkoutFinishedPage.TestOrderItems.First();
            Assert.AreEqual("Programming Book", firstItem.Bookname);
            Assert.AreEqual(15.99m, firstItem.Price);
            Assert.AreEqual(1, firstItem.Quantity);
            Assert.AreEqual(15.99m, firstItem.Total);
        }

        [TestMethod]
        public async Task CheckoutWorkflow_WithEmptyCart_ShouldRedirectToShoppingCart()
        {
            // Arrange
            var userId = "test-user-123";
            var correlationId = "cart-correlation-123";

            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync((Domain.Carts.ShoppingCart)null);

            var checkoutPage = new TestableCheckoutPage
            {
                AddressService = _mockAddressService.Object,
                ShoppingCartService = _mockShoppingCartService.Object,
                OrderService = _mockOrderService.Object
            };
            checkoutPage.SetHttpContext(_mockHttpContext.Object);
            checkoutPage.SetCurrentUserId(userId);

            bool redirectCalled = false;
            _mockHttpResponse.Setup(r => r.Redirect("~/ShoppingCart.aspx"))
                .Callback(() => redirectCalled = true);

            // Act
            await checkoutPage.TestLoadCheckoutDataAsync();

            // Assert
            Assert.IsTrue(redirectCalled);
        }

        [TestMethod]
        public async Task CheckoutWorkflow_WithNoAddresses_ShouldDisableCheckout()
        {
            // Arrange
            var userId = "test-user-123";
            var correlationId = "cart-correlation-123";

            var addresses = new List<Domain.Addresses.Address>();
            var shoppingCart = CreateTestShoppingCart();

            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _mockAddressService.Setup(s => s.GetAddressesAsync(userId)).ReturnsAsync(addresses);
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync(correlationId)).ReturnsAsync(shoppingCart);

            var checkoutPage = new TestableCheckoutPage
            {
                AddressService = _mockAddressService.Object,
                ShoppingCartService = _mockShoppingCartService.Object,
                OrderService = _mockOrderService.Object
            };
            checkoutPage.SetHttpContext(_mockHttpContext.Object);
            checkoutPage.SetCurrentUserId(userId);

            // Act
            await checkoutPage.TestLoadCheckoutDataAsync();

            // Assert
            Assert.AreEqual(0, checkoutPage.TestAddresses.Count);
            Assert.AreEqual(0, checkoutPage.TestSelectedAddressId);
        }

        [TestMethod]
        public async Task CheckoutWorkflow_WithOrderCreationFailure_ShouldShowError()
        {
            // Arrange
            var userId = "test-user-123";
            var correlationId = "cart-correlation-123";
            var addressId = 1;

            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _mockOrderService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDto>()))
                .ThrowsAsync(new Exception("Order creation failed"));

            var checkoutPage = new TestableCheckoutPage
            {
                AddressService = _mockAddressService.Object,
                ShoppingCartService = _mockShoppingCartService.Object,
                OrderService = _mockOrderService.Object
            };
            checkoutPage.SetHttpContext(_mockHttpContext.Object);
            checkoutPage.SetCurrentUserId(userId);

            // Act
            await checkoutPage.TestFinishOrder(addressId);

            // Assert
            Assert.IsTrue(checkoutPage.TestErrorShown);
            Assert.AreEqual("An error occurred while processing your order. Please try again.", checkoutPage.TestErrorMessage);
        }

        [TestMethod]
        public async Task CheckoutFinishedWorkflow_WithUnauthorizedAccess_ShouldRedirectToUnauthorized()
        {
            // Arrange
            var userId = "test-user-123";
            var differentUserId = "different-user-456";
            var orderId = 789;
            var order = CreateTestOrder(differentUserId, orderId);

            _mockHttpRequest.Setup(r => r.QueryString["orderId"]).Returns(orderId.ToString());
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId)).ReturnsAsync(order);

            var checkoutFinishedPage = new TestableCheckoutFinishedPage
            {
                OrderService = _mockOrderService.Object
            };
            checkoutFinishedPage.SetHttpContext(_mockHttpContext.Object);
            checkoutFinishedPage.SetCurrentUserId(userId);

            bool redirectCalled = false;
            _mockHttpResponse.Setup(r => r.Redirect("~/Unauthorized.aspx"))
                .Callback(() => redirectCalled = true);

            // Act
            await checkoutFinishedPage.TestLoadOrderDetailsAsync();

            // Assert
            Assert.IsTrue(redirectCalled);
        }

        private List<Domain.Addresses.Address> CreateTestAddresses()
        {
            return new List<Domain.Addresses.Address>
            {
                new Domain.Addresses.Address
                {
                    Id = 1,
                    AddressLine1 = "123 Main St",
                    AddressLine2 = "Apt 4B",
                    City = "Test City",
                    State = "TS",
                    Country = "USA",
                    ZipCode = "12345",
                    IsPrimary = true
                },
                new Domain.Addresses.Address
                {
                    Id = 2,
                    AddressLine1 = "456 Oak Ave",
                    AddressLine2 = "",
                    City = "Another City",
                    State = "AC",
                    Country = "USA",
                    ZipCode = "67890",
                    IsPrimary = false
                }
            };
        }

        private Domain.Carts.ShoppingCart CreateTestShoppingCart()
        {
            var mockShoppingCart = new Mock<Domain.Carts.ShoppingCart>();
            var shoppingCartItems = new List<Domain.Carts.ShoppingCartItem>();

            // Create first book and cart item
            var mockBook1 = new Mock<Domain.Books.Book>();
            mockBook1.Setup(b => b.Name).Returns("Programming Book");
            mockBook1.Setup(b => b.Price).Returns(15.99m);
            mockBook1.Setup(b => b.CoverImageUrl).Returns("/images/programming.jpg");
            mockBook1.Setup(b => b.Quantity).Returns(10);

            var mockCartItem1 = new Mock<Domain.Carts.ShoppingCartItem>();
            mockCartItem1.Setup(i => i.Book).Returns(mockBook1.Object);
            shoppingCartItems.Add(mockCartItem1.Object);

            // Create second book and cart item
            var mockBook2 = new Mock<Domain.Books.Book>();
            mockBook2.Setup(b => b.Name).Returns("Design Patterns");
            mockBook2.Setup(b => b.Price).Returns(10.99m);
            mockBook2.Setup(b => b.CoverImageUrl).Returns("/images/patterns.jpg");
            mockBook2.Setup(b => b.Quantity).Returns(5);

            var mockCartItem2 = new Mock<Domain.Carts.ShoppingCartItem>();
            mockCartItem2.Setup(i => i.Book).Returns(mockBook2.Object);
            shoppingCartItems.Add(mockCartItem2.Object);

            mockShoppingCart.Setup(sc => sc.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems))
                .Returns(shoppingCartItems);
            
            mockShoppingCart.Setup(sc => sc.GetSubTotal(ShoppingCartItemFilter.ExcludeOutOfStockItems))
                .Returns(26.98m);

            return mockShoppingCart.Object;
        }

        private Order CreateTestOrder(string userId, int orderId)
        {
            var mockOrder = new Mock<Order>();
            var mockCustomer = new Mock<Domain.Customers.Customer>();
            var mockOrderItems = new List<OrderItem>();

            // Create mock order items
            var mockBook1 = new Mock<Domain.Books.Book>();
            mockBook1.Setup(b => b.Id).Returns(1);
            mockBook1.Setup(b => b.Name).Returns("Programming Book");
            mockBook1.Setup(b => b.Price).Returns(15.99m);
            mockBook1.Setup(b => b.CoverImageUrl).Returns("/images/programming.jpg");

            var mockBook2 = new Mock<Domain.Books.Book>();
            mockBook2.Setup(b => b.Id).Returns(2);
            mockBook2.Setup(b => b.Name).Returns("Design Patterns");
            mockBook2.Setup(b => b.Price).Returns(10.99m);
            mockBook2.Setup(b => b.CoverImageUrl).Returns("/images/patterns.jpg");

            var mockOrderItem1 = new Mock<OrderItem>();
            mockOrderItem1.Setup(oi => oi.Book).Returns(mockBook1.Object);
            mockOrderItem1.Setup(oi => oi.Quantity).Returns(1);

            var mockOrderItem2 = new Mock<OrderItem>();
            mockOrderItem2.Setup(oi => oi.Book).Returns(mockBook2.Object);
            mockOrderItem2.Setup(oi => oi.Quantity).Returns(1);

            mockOrderItems.Add(mockOrderItem1.Object);
            mockOrderItems.Add(mockOrderItem2.Object);

            mockCustomer.Setup(c => c.Sub).Returns(userId);
            mockOrder.Setup(o => o.Id).Returns(orderId);
            mockOrder.Setup(o => o.Customer).Returns(mockCustomer.Object);
            mockOrder.Setup(o => o.OrderItems).Returns(mockOrderItems);

            return mockOrder.Object;
        }
    }
}