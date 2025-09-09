using System;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Bookstore.Domain.Addresses;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Web;
using System.Collections.Specialized;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class AddressCreateUpdatePageTests
    {
        private Mock<IAddressService> _mockAddressService;
        private AddressCreateUpdate _addressCreateUpdatePage;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;
        private Mock<HttpServerUtilityBase> _mockServer;

        [TestInitialize]
        public void Setup()
        {
            _mockAddressService = new Mock<IAddressService>();
            _addressCreateUpdatePage = new AddressCreateUpdate();
            _addressCreateUpdatePage.AddressService = _mockAddressService.Object;

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

            // Setup query string
            var queryString = new NameValueCollection();
            _mockRequest.Setup(r => r.QueryString).Returns(queryString);
        }

        [TestMethod]
        public async Task LoadAddressAsync_WithValidAddressId_LoadsAddressSuccessfully()
        {
            // Arrange
            var address = new Domain.Addresses.Address
            {
                Id = 123,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 4B",
                City = "Anytown",
                State = "CA",
                Country = "USA",
                ZipCode = "12345"
            };

            _mockRequest.Setup(r => r.QueryString["id"]).Returns("123");
            _mockAddressService.Setup(s => s.GetAddressAsync("test-user-123", 123))
                .ReturnsAsync(address);

            // Act
            await _addressCreateUpdatePage.LoadAddressAsync();

            // Assert
            _mockAddressService.Verify(s => s.GetAddressAsync("test-user-123", 123), Times.Once);
        }

        [TestMethod]
        public async Task SaveAddressAsync_CreateNewAddress_CreatesAddressSuccessfully()
        {
            // Arrange
            _mockRequest.Setup(r => r.QueryString["id"]).Returns((string)null);
            _mockAddressService.Setup(s => s.CreateAddressAsync(It.IsAny<CreateAddressDto>()))
                .Returns(Task.CompletedTask);

            // Setup form controls
            _addressCreateUpdatePage.AddressLine1TextBox = new TextBox { Text = "123 Main St" };
            _addressCreateUpdatePage.AddressLine2TextBox = new TextBox { Text = "Apt 4B" };
            _addressCreateUpdatePage.CityTextBox = new TextBox { Text = "Anytown" };
            _addressCreateUpdatePage.StateDropDownList = new DropDownList();
            _addressCreateUpdatePage.StateDropDownList.Items.Add(new ListItem("California", "CA"));
            _addressCreateUpdatePage.StateDropDownList.SelectedValue = "CA";
            _addressCreateUpdatePage.CountryTextBox = new TextBox { Text = "USA" };
            _addressCreateUpdatePage.ZipCodeTextBox = new TextBox { Text = "12345" };

            // Act
            await _addressCreateUpdatePage.SaveAddressAsync();

            // Assert
            _mockAddressService.Verify(s => s.CreateAddressAsync(It.Is<CreateAddressDto>(dto =>
                dto.AddressLine1 == "123 Main St" &&
                dto.AddressLine2 == "Apt 4B" &&
                dto.City == "Anytown" &&
                dto.State == "CA" &&
                dto.Country == "USA" &&
                dto.ZipCode == "12345" &&
                dto.UserId == "test-user-123")), Times.Once);
        }

        [TestMethod]
        public async Task SaveAddressAsync_UpdateExistingAddress_UpdatesAddressSuccessfully()
        {
            // Arrange
            _mockRequest.Setup(r => r.QueryString["id"]).Returns("123");
            _mockAddressService.Setup(s => s.UpdateAddressAsync(It.IsAny<UpdateAddressDto>()))
                .Returns(Task.CompletedTask);

            // Setup form controls
            _addressCreateUpdatePage.AddressLine1TextBox = new TextBox { Text = "456 Oak Ave" };
            _addressCreateUpdatePage.AddressLine2TextBox = new TextBox { Text = "Suite 200" };
            _addressCreateUpdatePage.CityTextBox = new TextBox { Text = "Another City" };
            _addressCreateUpdatePage.StateDropDownList = new DropDownList();
            _addressCreateUpdatePage.StateDropDownList.Items.Add(new ListItem("New York", "NY"));
            _addressCreateUpdatePage.StateDropDownList.SelectedValue = "NY";
            _addressCreateUpdatePage.CountryTextBox = new TextBox { Text = "USA" };
            _addressCreateUpdatePage.ZipCodeTextBox = new TextBox { Text = "67890" };

            // Act
            await _addressCreateUpdatePage.SaveAddressAsync();

            // Assert
            _mockAddressService.Verify(s => s.UpdateAddressAsync(It.Is<UpdateAddressDto>(dto =>
                dto.Id == 123 &&
                dto.AddressLine1 == "456 Oak Ave" &&
                dto.AddressLine2 == "Suite 200" &&
                dto.City == "Another City" &&
                dto.State == "NY" &&
                dto.Country == "USA" &&
                dto.ZipCode == "67890" &&
                dto.UserId == "test-user-123")), Times.Once);
        }

        [TestMethod]
        public async Task SaveAddressAsync_WithServiceException_HandlesErrorGracefully()
        {
            // Arrange
            _mockRequest.Setup(r => r.QueryString["id"]).Returns((string)null);
            _mockAddressService.Setup(s => s.CreateAddressAsync(It.IsAny<CreateAddressDto>()))
                .ThrowsAsync(new Exception("Service error"));

            // Setup form controls
            _addressCreateUpdatePage.AddressLine1TextBox = new TextBox { Text = "123 Main St" };
            _addressCreateUpdatePage.AddressLine2TextBox = new TextBox { Text = "" };
            _addressCreateUpdatePage.CityTextBox = new TextBox { Text = "Anytown" };
            _addressCreateUpdatePage.StateDropDownList = new DropDownList();
            _addressCreateUpdatePage.StateDropDownList.Items.Add(new ListItem("California", "CA"));
            _addressCreateUpdatePage.StateDropDownList.SelectedValue = "CA";
            _addressCreateUpdatePage.CountryTextBox = new TextBox { Text = "USA" };
            _addressCreateUpdatePage.ZipCodeTextBox = new TextBox { Text = "12345" };

            // Act
            await _addressCreateUpdatePage.SaveAddressAsync();

            // Assert
            _mockAddressService.Verify(s => s.CreateAddressAsync(It.IsAny<CreateAddressDto>()), Times.Once);
            // Error should be handled gracefully
        }
    }
}