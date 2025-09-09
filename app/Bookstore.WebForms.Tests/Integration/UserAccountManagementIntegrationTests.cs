using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Orders;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Books;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Web;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class UserAccountManagementIntegrationTests
    {
        private Mock<IOrderService> _mockOrderService;
        private Mock<IAddressService> _mockAddressService;
        private Mock<IShoppingCartService> _mockShoppingCartService;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;
        private Mock<HttpServerUtilityBase> _mockServer;
        private Mock<HttpSessionStateBase> _mockSession;

        [TestInitialize]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockAddressService = new Mock<IAddressService>();
            _mockShoppingCartService = new Mock<IShoppingCartService>();

            // Setup mock HTTP context
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockRequest = new Mock<HttpRequestBase>();
            _mockResponse = new Mock<HttpResponseBase>();
            _mockServer = new Mock<HttpServerUtilityBase>();
            _mockSession = new Mock<HttpSessionStateBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
            _mockHttpContext.Setup(c => c.Server).Returns(_mockServer.Object);
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
            _mockServer.Setup(s => s.UrlEncode(It.IsAny<string>())).Returns<string>(s => s);

            // Setup authenticated user
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-123")
            }, "test");
            var principal = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(principal);

            _mockSession.Setup(s => s["ShoppingCartCorrelationId"]).Returns("test-correlation-id");
        }

        [TestMethod]
        public async Task UserAccountWorkflow_CompleteUserJourney_WorksEndToEnd()
        {
            // Arrange - Setup test data
            var orders = new List<Order>
            {
                new Order { Id = 1, SubTotal = 25.99m, DeliveryDate = DateTime.Now.AddDays(3), OrderStatus = OrderStatus.Processing }
            };

            var addresses = new List<Domain.Addresses.Address>
            {
                new Domain.Addresses.Address 
                { 
                    Id = 1, 
                    AddressLine1 = "123 Main St", 
                    City = "Anytown", 
                    State = "CA", 
                    Country = "USA", 
                    ZipCode = "12345" 
                }
            };

            var book = new Book { Id = 1, Name = "Test Book", CoverImageUrl = "image.jpg", Price = 19.99m };
            var wishlistItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { Id = 1, Book = book, IsWishListItem = true }
            };

            var shoppingCart = new ShoppingCart();
            foreach (var item in wishlistItems)
            {
                shoppingCart.AddItem(item);
            }

            // Setup service mocks
            _mockOrderService.Setup(s => s.GetOrdersAsync("test-user-123")).ReturnsAsync(orders);
            _mockAddressService.Setup(s => s.GetAddressesAsync("test-user-123")).ReturnsAsync(addresses);
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync("test-correlation-id")).ReturnsAsync(shoppingCart);

            // Act & Assert - Test Orders page
            var ordersPage = new Orders();
            ordersPage.OrderService = _mockOrderService.Object;
            await ordersPage.LoadOrdersAsync();

            _mockOrderService.Verify(s => s.GetOrdersAsync("test-user-123"), Times.Once);

            // Act & Assert - Test Address page
            var addressPage = new Address();
            addressPage.AddressService = _mockAddressService.Object;
            await addressPage.LoadAddressesAsync();

            _mockAddressService.Verify(s => s.GetAddressesAsync("test-user-123"), Times.Once);

            // Act & Assert - Test Wishlist page
            var wishlistPage = new Wishlist();
            wishlistPage.ShoppingCartService = _mockShoppingCartService.Object;
            await wishlistPage.LoadWishlistAsync();

            _mockShoppingCartService.Verify(s => s.GetShoppingCartAsync("test-correlation-id"), Times.Once);
        }

        [TestMethod]
        public async Task UserAccountWorkflow_WithServiceErrors_HandlesErrorsGracefully()
        {
            // Arrange - Setup service mocks to throw exceptions
            _mockOrderService.Setup(s => s.GetOrdersAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Orders service error"));
            _mockAddressService.Setup(s => s.GetAddressesAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Address service error"));
            _mockShoppingCartService.Setup(s => s.GetShoppingCartAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Shopping cart service error"));

            // Act & Assert - Test Orders page error handling
            var ordersPage = new Orders();
            ordersPage.OrderService = _mockOrderService.Object;
            
            try
            {
                await ordersPage.LoadOrdersAsync();
                // Should handle error gracefully without throwing
            }
            catch (Exception)
            {
                Assert.Fail("Orders page should handle service errors gracefully");
            }

            // Act & Assert - Test Address page error handling
            var addressPage = new Address();
            addressPage.AddressService = _mockAddressService.Object;
            
            try
            {
                await addressPage.LoadAddressesAsync();
                // Should handle error gracefully without throwing
            }
            catch (Exception)
            {
                Assert.Fail("Address page should handle service errors gracefully");
            }

            // Act & Assert - Test Wishlist page error handling
            var wishlistPage = new Wishlist();
            wishlistPage.ShoppingCartService = _mockShoppingCartService.Object;
            
            try
            {
                await wishlistPage.LoadWishlistAsync();
                // Should handle error gracefully without throwing
            }
            catch (Exception)
            {
                Assert.Fail("Wishlist page should handle service errors gracefully");
            }
        }

        [TestMethod]
        public async Task UserAccountWorkflow_WithUnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange - Setup unauthenticated user
            var identity = new ClaimsIdentity(); // No claims = unauthenticated
            var principal = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(principal);

            var redirectUrl = "";
            _mockResponse.Setup(r => r.Redirect(It.IsAny<string>()))
                .Callback<string>(url => redirectUrl = url);

            // Act - Test Orders page with unauthenticated user
            var ordersPage = new Orders();
            ordersPage.OrderService = _mockOrderService.Object;
            await ordersPage.LoadOrdersAsync();

            // Assert - Should redirect to login
            Assert.IsTrue(redirectUrl.Contains("Login.aspx"), "Should redirect to login page");
        }

        [TestMethod]
        public async Task AddressManagement_CreateUpdateDeleteWorkflow_WorksCorrectly()
        {
            // Arrange
            var existingAddress = new Domain.Addresses.Address
            {
                Id = 123,
                AddressLine1 = "123 Main St",
                City = "Anytown",
                State = "CA",
                Country = "USA",
                ZipCode = "12345"
            };

            _mockAddressService.Setup(s => s.GetAddressAsync("test-user-123", 123))
                .ReturnsAsync(existingAddress);
            _mockAddressService.Setup(s => s.CreateAddressAsync(It.IsAny<CreateAddressDto>()))
                .Returns(Task.CompletedTask);
            _mockAddressService.Setup(s => s.UpdateAddressAsync(It.IsAny<UpdateAddressDto>()))
                .Returns(Task.CompletedTask);
            _mockAddressService.Setup(s => s.DeleteAddressAsync(It.IsAny<DeleteAddressDto>()))
                .Returns(Task.CompletedTask);

            // Act & Assert - Test address creation
            var createUpdatePage = new AddressCreateUpdate();
            createUpdatePage.AddressService = _mockAddressService.Object;
            
            // Simulate creating a new address
            await createUpdatePage.SaveAddressAsync();
            _mockAddressService.Verify(s => s.CreateAddressAsync(It.IsAny<CreateAddressDto>()), Times.Once);

            // Act & Assert - Test address loading for update
            _mockRequest.Setup(r => r.QueryString["id"]).Returns("123");
            await createUpdatePage.LoadAddressAsync();
            _mockAddressService.Verify(s => s.GetAddressAsync("test-user-123", 123), Times.Once);

            // Act & Assert - Test address deletion
            var addressPage = new Address();
            addressPage.AddressService = _mockAddressService.Object;
            await addressPage.DeleteAddressAsync(123);
            _mockAddressService.Verify(s => s.DeleteAddressAsync(It.IsAny<DeleteAddressDto>()), Times.Once);
        }
    }
}