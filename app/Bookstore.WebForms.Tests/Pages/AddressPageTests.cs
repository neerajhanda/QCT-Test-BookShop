using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Bookstore.Domain.Addresses;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Web;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class AddressPageTests
    {
        private Mock<IAddressService> _mockAddressService;
        private Address _addressPage;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;
        private Mock<HttpServerUtilityBase> _mockServer;

        [TestInitialize]
        public void Setup()
        {
            _mockAddressService = new Mock<IAddressService>();
            _addressPage = new Address();
            _addressPage.AddressService = _mockAddressService.Object;

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
        public async Task LoadAddressesAsync_WithValidUser_LoadsAddressesSuccessfully()
        {
            // Arrange
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
                },
                new Domain.Addresses.Address 
                { 
                    Id = 2, 
                    AddressLine1 = "456 Oak Ave", 
                    City = "Another City", 
                    State = "NY", 
                    Country = "USA", 
                    ZipCode = "67890" 
                }
            };

            _mockAddressService.Setup(s => s.GetAddressesAsync("test-user-123"))
                .ReturnsAsync(addresses);

            // Act
            await _addressPage.LoadAddressesAsync();

            // Assert
            _mockAddressService.Verify(s => s.GetAddressesAsync("test-user-123"), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAddressAsync_WithValidAddress_DeletesAddressSuccessfully()
        {
            // Arrange
            int addressId = 123;
            _mockAddressService.Setup(s => s.DeleteAddressAsync(It.IsAny<DeleteAddressDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _addressPage.DeleteAddressAsync(addressId);

            // Assert
            _mockAddressService.Verify(s => s.DeleteAddressAsync(It.Is<DeleteAddressDto>(dto => 
                dto.AddressId == addressId && dto.UserId == "test-user-123")), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAddressAsync_WithServiceException_HandlesErrorGracefully()
        {
            // Arrange
            int addressId = 123;
            _mockAddressService.Setup(s => s.DeleteAddressAsync(It.IsAny<DeleteAddressDto>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            await _addressPage.DeleteAddressAsync(addressId);

            // Assert
            _mockAddressService.Verify(s => s.DeleteAddressAsync(It.IsAny<DeleteAddressDto>()), Times.Once);
            // Error should be handled gracefully
        }
    }
}