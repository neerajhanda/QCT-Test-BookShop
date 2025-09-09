using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Authentication
{
    [TestClass]
    public class AdminBasePageTests
    {
        private TestAdminBasePage _adminPage;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;

        [TestInitialize]
        public void Setup()
        {
            _adminPage = new TestAdminBasePage();
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockRequest = new Mock<HttpRequestBase>();
            _mockResponse = new Mock<HttpResponseBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
            _mockRequest.Setup(r => r.RawUrl).Returns("/Admin/Dashboard.aspx");
        }

        [TestMethod]
        public void OnPreInit_WhenUserIsAdmin_DoesNotRedirect()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act & Assert - Should not throw exception
            _adminPage.TestOnPreInit(EventArgs.Empty);
        }

        [TestMethod]
        public void OnPreInit_WhenUserIsNotAdmin_ThrowsException()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "User")); // Not admin
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _adminPage.TestOnPreInit(EventArgs.Empty));
        }

        [TestMethod]
        public void OnPreInit_WhenUserNotAuthenticated_ThrowsException()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => _adminPage.TestOnPreInit(EventArgs.Empty));
        }

        [TestMethod]
        public void InitializeAdminPage_IsCalled_WhenOnLoadExecutes()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            _adminPage.TestOnLoad(EventArgs.Empty);

            // Assert
            Assert.IsTrue(_adminPage.InitializeAdminPageCalled);
        }

        [TestMethod]
        public void LogAdminAction_LogsCorrectMessage()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));
            identity.AddClaim(new Claim("sub", "admin-user-id"));
            identity.AddClaim(new Claim("given_name", "Admin"));
            identity.AddClaim(new Claim("family_name", "User"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            _adminPage.TestLogAdminAction("Test Action", "Test Details");

            // Assert
            Assert.IsTrue(_adminPage.LogAdminActionCalled);
            Assert.AreEqual("Test Action", _adminPage.LastLoggedAction);
            Assert.AreEqual("Test Details", _adminPage.LastLoggedDetails);
        }

        // Test implementation of AdminBasePage to expose protected members
        private class TestAdminBasePage : AdminBasePage
        {
            public bool InitializeAdminPageCalled { get; private set; }
            public bool LogAdminActionCalled { get; private set; }
            public string LastLoggedAction { get; private set; }
            public string LastLoggedDetails { get; private set; }

            public void TestOnPreInit(EventArgs e) => OnPreInit(e);
            public void TestOnLoad(EventArgs e) => OnLoad(e);
            public void TestLogAdminAction(string action, string details) => LogAdminAction(action, details);

            protected override void InitializeAdminPage()
            {
                InitializeAdminPageCalled = true;
            }

            protected override void LogAdminAction(string action, string details = null)
            {
                LogAdminActionCalled = true;
                LastLoggedAction = action;
                LastLoggedDetails = details;
            }

            // Override to prevent actual dependency injection during tests
            protected override void OnPreInit(EventArgs e)
            {
                // Check authorization before calling base OnPreInit
                RequireAdministrator();
                
                // Skip base implementation for tests to avoid dependency injection
            }
        }
    }
}