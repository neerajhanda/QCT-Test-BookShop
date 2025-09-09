using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Authentication
{
    [TestClass]
    public class BasePageAuthenticationTests
    {
        private TestBasePage _basePage;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;

        [TestInitialize]
        public void Setup()
        {
            _basePage = new TestBasePage();
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockRequest = new Mock<HttpRequestBase>();
            _mockResponse = new Mock<HttpResponseBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
            _mockRequest.Setup(r => r.RawUrl).Returns("/test-page.aspx");
        }

        [TestMethod]
        public void IsUserAuthenticated_WhenUserIsAuthenticated_ReturnsTrue()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Name, "testuser"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            var result = _basePage.TestIsUserAuthenticated;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsUserAuthenticated_WhenUserIsNotAuthenticated_ReturnsFalse()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            var result = _basePage.TestIsUserAuthenticated;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CurrentUserName_WhenUserIsAuthenticated_ReturnsDisplayName()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim("given_name", "John"));
            identity.AddClaim(new Claim("family_name", "Doe"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            var result = _basePage.TestCurrentUserName;

            // Assert
            Assert.AreEqual("John Doe", result);
        }

        [TestMethod]
        public void CurrentUserId_WhenUserIsAuthenticated_ReturnsUserId()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim("sub", "test-user-id"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            var result = _basePage.TestCurrentUserId;

            // Assert
            Assert.AreEqual("test-user-id", result);
        }

        [TestMethod]
        public void IsUserInRole_WhenUserIsInRole_ReturnsTrue()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            var result = _basePage.TestIsUserInRole("Administrators");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsUserAdministrator_WhenUserIsAdmin_ReturnsTrue()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            var result = _basePage.TestIsUserAdministrator;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RequireAuthentication_WhenUserNotAuthenticated_RedirectsToLogin()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _basePage.TestRequireAuthentication());
        }

        // Test implementation of BasePage to expose protected members
        private class TestBasePage : BasePage
        {
            public bool TestIsUserAuthenticated => IsUserAuthenticated;
            public string TestCurrentUserName => CurrentUserName;
            public string TestCurrentUserId => CurrentUserId;
            public bool TestIsUserAdministrator => IsUserAdministrator;
            
            public bool TestIsUserInRole(string role) => IsUserInRole(role);
            public void TestRequireAuthentication() => RequireAuthentication();
            public void TestRequireRole(string role) => RequireRole(role);
            public void TestRequireAdministrator() => RequireAdministrator();

            // Override to prevent actual dependency injection during tests
            protected override void OnPreInit(EventArgs e)
            {
                // Skip base implementation for tests
            }
        }
    }
}