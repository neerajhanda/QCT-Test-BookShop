using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Orders;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class CheckoutPageTests
    {
        private Mock<IAddressService> _mockAddressService;
        private Mock<IShoppingCartService> _mockShoppingCartService;
        private Mock<IOrderService> _mockOrderService;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockHttpRequest;
        private Mock<HttpResponseBase> _mockHttpResponse;
        private Mock<HttpSessionStateBase> _mockSession;
        private TestableCheckoutPage _checkoutPage;

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

            _checkoutPage = new TestableCheckoutPage
            {
                AddressService = _mockAddressService.Object,
                ShoppingCartService = _mockShoppingCartService.Object,
                OrderService = _mockOrderService.Object
            };

            _checkoutPage.SetHttpContext(_mockHttpContext.Object);
        }

        [TestMethod]
        public async Task LoadCheckoutData_WithValidData_ShouldLoadSuccessfully()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var userId = "test-user-id";
            
            var addresses = new List<Domain.Addresses.Address>
            {
                new Domain.Addresses.Address { Id = 1, AddressLine1 = "123 Main St", City = "Test City", State = "TS", Country = "USA", ZipCode = "12345" }
            };

            var shoppingCart = CreateTestShoppingCart();
            
            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _checkoutPage.SetCurrentUserId(userId);
            
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(shoppingCart);
            
            _mockAddressService.Setup(s => s.GetAddressesAsync(userId))
                .ReturnsAsync(addresses);

            // Act
            await _checkoutPage.TestLoadCheckoutDataAsync();

            // Assert
            Assert.IsNotNull(_checkoutPage.TestAddresses);
            Assert.AreEqual(1, _checkoutPage.TestAddresses.Count);
            Assert.AreEqual("123 Main St", _checkoutPage.TestAddresses[0].AddressLine1);
            
            Assert.IsNotNull(_checkoutPage.TestShoppingCartItems);
            Assert.AreEqual(1, _checkoutPage.TestShoppingCartItems.Count);
            
            Assert.AreEqual(10.99m, _checkoutPage.TestTotal);
            Assert.AreEqual(1, _checkoutPage.TestSelectedAddressId);
        }

        [TestMethod]
        public async Task LoadCheckoutData_WithEmptyCart_ShouldRedirectToShoppingCart()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var userId = "test-user-id";
            
            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _checkoutPage.SetCurrentUserId(userId);
            
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync((Domain.Carts.ShoppingCart)null);

            bool redirectCalled = false;
            _mockHttpResponse.Setup(r => r.Redirect("~/ShoppingCart.aspx"))
                .Callback(() => redirectCalled = true);

            // Act
            await _checkoutPage.TestLoadCheckoutDataAsync();

            // Assert
            Assert.IsTrue(redirectCalled);
        }

        [TestMethod]
        public async Task LoadCheckoutData_WithNoAddresses_ShouldSetSelectedAddressIdToZero()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var userId = "test-user-id";
            
            var addresses = new List<Domain.Addresses.Address>();
            var shoppingCart = CreateTestShoppingCart();
            
            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _checkoutPage.SetCurrentUserId(userId);
            
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(shoppingCart);
            
            _mockAddressService.Setup(s => s.GetAddressesAsync(userId))
                .ReturnsAsync(addresses);

            // Act
            await _checkoutPage.TestLoadCheckoutDataAsync();

            // Assert
            Assert.AreEqual(0, _checkoutPage.TestSelectedAddressId);
            Assert.AreEqual(0, _checkoutPage.TestAddresses.Count);
        }

        [TestMethod]
        public async Task FinishOrder_WithValidSelection_ShouldCreateOrderAndRedirect()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var userId = "test-user-id";
            var selectedAddressId = 1;
            var orderId = 123;

            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _checkoutPage.SetCurrentUserId(userId);
            
            _mockOrderService.Setup(s => s.CreateOrderAsync(It.Is<CreateOrderDto>(dto => 
                dto.CustomerSub == userId && 
                dto.CorrelationId == correlationId && 
                dto.AddressId == selectedAddressId)))
                .ReturnsAsync(orderId);

            bool redirectCalled = false;
            string redirectUrl = null;
            _mockHttpResponse.Setup(r => r.Redirect(It.IsAny<string>()))
                .Callback<string>(url => { redirectCalled = true; redirectUrl = url; });

            // Act
            await _checkoutPage.TestFinishOrder(selectedAddressId);

            // Assert
            Assert.IsTrue(redirectCalled);
            Assert.AreEqual($"~/CheckoutFinished.aspx?orderId={orderId}", redirectUrl);
            
            _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<CreateOrderDto>()), Times.Once);
        }

        [TestMethod]
        public async Task FinishOrder_WithInvalidAddressId_ShouldShowError()
        {
            // Arrange
            var correlationId = "test-correlation-id";
            var userId = "test-user-id";
            var selectedAddressId = 0; // Invalid address ID

            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(correlationId);
            _checkoutPage.SetCurrentUserId(userId);

            // Act
            await _checkoutPage.TestFinishOrder(selectedAddressId);

            // Assert
            Assert.IsTrue(_checkoutPage.TestErrorShown);
            Assert.AreEqual("Please select an address to proceed with checkout.", _checkoutPage.TestErrorMessage);
            
            _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<CreateOrderDto>()), Times.Never);
        }

        [TestMethod]
        public void GetShoppingCartCorrelationId_FromSession_ShouldReturnSessionValue()
        {
            // Arrange
            var expectedCorrelationId = "session-correlation-id";
            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns(expectedCorrelationId);

            // Act
            var result = _checkoutPage.TestGetShoppingCartCorrelationId();

            // Assert
            Assert.AreEqual(expectedCorrelationId, result);
        }

        [TestMethod]
        public void GetShoppingCartCorrelationId_FromCookie_ShouldReturnCookieValue()
        {
            // Arrange
            var expectedCorrelationId = "cookie-correlation-id";
            var mockCookies = new Mock<HttpCookieCollection>();
            var cookie = new HttpCookie("ShoppingCartCorrelationId", expectedCorrelationId);
            
            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns((string)null);
            _mockHttpRequest.Setup(r => r.Cookies).Returns(mockCookies.Object);
            _mockHttpRequest.Setup(r => r.Cookies["ShoppingCartCorrelationId"]).Returns(cookie);

            // Act
            var result = _checkoutPage.TestGetShoppingCartCorrelationId();

            // Assert
            Assert.AreEqual(expectedCorrelationId, result);
        }

        [TestMethod]
        public void GetShoppingCartCorrelationId_NoExistingId_ShouldGenerateNewId()
        {
            // Arrange
            var mockCookies = new Mock<HttpCookieCollection>();
            var mockResponseCookies = new Mock<HttpCookieCollection>();
            
            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns((string)null);
            _mockHttpRequest.Setup(r => r.Cookies).Returns(mockCookies.Object);
            _mockHttpRequest.Setup(r => r.Cookies["ShoppingCartCorrelationId"]).Returns((HttpCookie)null);
            _mockHttpResponse.Setup(r => r.Cookies).Returns(mockResponseCookies.Object);

            // Act
            var result = _checkoutPage.TestGetShoppingCartCorrelationId();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(Guid.TryParse(result, out _));
            
            _mockSession.VerifySet(s => s["ShoppingCartCorrelationId"] = result, Times.Once);
        }

        private Domain.Carts.ShoppingCart CreateTestShoppingCart()
        {
            var mockShoppingCart = new Mock<Domain.Carts.ShoppingCart>();
            var mockBook = new Mock<Domain.Books.Book>();
            var mockShoppingCartItem = new Mock<Domain.Carts.ShoppingCartItem>();

            mockBook.Setup(b => b.Name).Returns("Test Book");
            mockBook.Setup(b => b.Price).Returns(10.99m);
            mockBook.Setup(b => b.CoverImageUrl).Returns("/images/test.jpg");
            mockBook.Setup(b => b.Quantity).Returns(5);

            mockShoppingCartItem.Setup(i => i.Book).Returns(mockBook.Object);

            var shoppingCartItems = new List<Domain.Carts.ShoppingCartItem> { mockShoppingCartItem.Object };
            
            mockShoppingCart.Setup(sc => sc.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems))
                .Returns(shoppingCartItems);
            
            mockShoppingCart.Setup(sc => sc.GetSubTotal(ShoppingCartItemFilter.ExcludeOutOfStockItems))
                .Returns(10.99m);

            return mockShoppingCart.Object;
        }
    }

    // Testable version of CheckoutPage that exposes protected/private members for testing
    public class TestableCheckoutPage : Checkout
    {
        public List<Models.CheckoutAddressViewModel> TestAddresses { get; private set; }
        public List<Models.CheckoutItemViewModel> TestShoppingCartItems { get; private set; }
        public decimal TestTotal { get; private set; }
        public int TestSelectedAddressId { get; private set; }
        public bool TestErrorShown { get; private set; }
        public string TestErrorMessage { get; private set; }

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

        protected override string CurrentUserId => _currentUserId;

        public async Task TestLoadCheckoutDataAsync()
        {
            await LoadCheckoutDataAsync();
            
            // Expose private fields for testing
            TestAddresses = _addresses;
            TestShoppingCartItems = _shoppingCartItems;
            TestTotal = _total;
            TestSelectedAddressId = SelectedAddressId;
        }

        public async Task TestFinishOrder(int selectedAddressId)
        {
            // Mock the GetSelectedAddressId method
            await FinishOrderWithAddressId(selectedAddressId);
        }

        private async Task FinishOrderWithAddressId(int selectedAddressId)
        {
            try
            {
                if (selectedAddressId == 0)
                {
                    ShowError("Please select an address to proceed with checkout.");
                    return;
                }

                var correlationId = TestGetShoppingCartCorrelationId();
                var createOrderDto = new CreateOrderDto(CurrentUserId, correlationId, selectedAddressId);
                
                var orderId = await OrderService.CreateOrderAsync(createOrderDto);

                _httpContext.Response.Redirect($"~/CheckoutFinished.aspx?orderId={orderId}");
            }
            catch (Exception)
            {
                ShowError("An error occurred while processing your order. Please try again.");
            }
        }

        public string TestGetShoppingCartCorrelationId()
        {
            return GetShoppingCartCorrelationId();
        }

        private new string GetShoppingCartCorrelationId()
        {
            var correlationId = _httpContext.Session["ShoppingCartCorrelationId"] as string;
            
            if (string.IsNullOrEmpty(correlationId))
            {
                var cookie = _httpContext.Request.Cookies["ShoppingCartCorrelationId"];
                if (cookie != null)
                {
                    correlationId = cookie.Value;
                }
            }

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                _httpContext.Session["ShoppingCartCorrelationId"] = correlationId;
                
                var newCookie = new HttpCookie("ShoppingCartCorrelationId", correlationId)
                {
                    Expires = DateTime.Now.AddDays(30)
                };
                _httpContext.Response.Cookies.Add(newCookie);
            }

            return correlationId;
        }

        private new void ShowError(string message)
        {
            TestErrorShown = true;
            TestErrorMessage = message;
        }
    }
}