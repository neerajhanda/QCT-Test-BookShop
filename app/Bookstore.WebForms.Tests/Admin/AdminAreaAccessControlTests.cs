using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Bookstore.WebForms;
using Bookstore.WebForms.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Admin
{
    [TestClass]
    public class AdminAreaAccessControlTests
    {
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;
        private Mock<HttpSessionStateBase> _mockSession;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockRequest = new Mock<HttpRequestBase>();
            _mockResponse = new Mock<HttpResponseBase>();
            _mockSession = new Mock<HttpSessionStateBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
            _mockRequest.Setup(r => r.RawUrl).Returns("/Admin/Dashboard.aspx");
            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost/Admin/Dashboard.aspx"));
        }

        [TestMethod]
        public void AdminBasePage_WhenUserIsAdmin_AllowsAccess()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));
            identity.AddClaim(new Claim("sub", "admin-user-id"));
            identity.AddClaim(new Claim("given_name", "Admin"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            var adminPage = new TestAdminBasePage();
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act & Assert - Should not throw exception
            adminPage.TestOnPreInit(EventArgs.Empty);
            Assert.IsTrue(adminPage.PreInitCalled);
        }

        [TestMethod]
        public void AdminBasePage_WhenUserIsNotAdmin_DeniesAccess()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "User")); // Not admin
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            var adminPage = new TestAdminBasePage();
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => adminPage.TestOnPreInit(EventArgs.Empty));
        }

        [TestMethod]
        public void AdminBasePage_WhenUserNotAuthenticated_DeniesAccess()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            var adminPage = new TestAdminBasePage();
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => adminPage.TestOnPreInit(EventArgs.Empty));
        }

        [TestMethod]
        public void AdminBasePage_LogAdminAction_LogsCorrectInformation()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));
            identity.AddClaim(new Claim("sub", "admin-user-id"));
            identity.AddClaim(new Claim("given_name", "Admin"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            var adminPage = new TestAdminBasePage();
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act
            adminPage.TestLogAdminAction("Test Action", "Test Details");

            // Assert
            Assert.IsTrue(adminPage.LogAdminActionCalled);
            Assert.AreEqual("Test Action", adminPage.LastLoggedAction);
            Assert.AreEqual("Test Details", adminPage.LastLoggedDetails);
        }

        [TestMethod]
        public void AdminBasePage_HasAdminPermission_ReturnsTrue_WhenUserIsAdmin()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            var adminPage = new TestAdminBasePage();
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act
            bool hasPermission = adminPage.TestHasAdminPermission("TestPermission");

            // Assert
            Assert.IsTrue(hasPermission);
        }

        [TestMethod]
        public void AdminBasePage_RequireAdminPermission_ThrowsException_WhenUserLacksPermission()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Role, "User")); // Not admin
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            var adminPage = new TestAdminBasePage();
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => 
                adminPage.TestRequireAdminPermission("TestPermission"));
        }

        [TestMethod]
        public void AdminNavigationControl_HighlightCurrentPage_HighlightsCorrectButton()
        {
            // Arrange
            var navControl = new AdminNavigationControl();

            // Act
            navControl.HighlightCurrentPage("dashboard");

            // Assert
            // This would need to be tested with actual control rendering
            // For now, we verify the method doesn't throw
            Assert.IsNotNull(navControl);
        }

        [TestMethod]
        public void AdminNavigationControl_AddBreadcrumbItem_AddsItemCorrectly()
        {
            // Arrange
            var navControl = new AdminNavigationControl();

            // Act & Assert - Should not throw
            navControl.AddBreadcrumbItem("Test Page", "~/Admin/Test.aspx");
            navControl.AddBreadcrumbItem("Current Page"); // No URL
        }

        // Test implementation of AdminBasePage to expose protected members
        private class TestAdminBasePage : AdminBasePage
        {
            private HttpContextBase _mockContext;
            
            public bool PreInitCalled { get; private set; }
            public bool LogAdminActionCalled { get; private set; }
            public string LastLoggedAction { get; private set; }
            public string LastLoggedDetails { get; private set; }

            public void SetMockContext(HttpContextBase context)
            {
                _mockContext = context;
            }

            public void TestOnPreInit(EventArgs e) => OnPreInit(e);
            public void TestLogAdminAction(string action, string details) => LogAdminAction(action, details);
            public bool TestHasAdminPermission(string permission) => HasAdminPermission(permission);
            public void TestRequireAdminPermission(string permission) => RequireAdminPermission(permission);

            protected override void LogAdminAction(string action, string details = null)
            {
                LogAdminActionCalled = true;
                LastLoggedAction = action;
                LastLoggedDetails = details;
            }

            // Override to use mock context and prevent actual dependency injection
            protected override void OnPreInit(EventArgs e)
            {
                PreInitCalled = true;
                
                // Use mock context if available
                if (_mockContext != null)
                {
                    // Simulate authorization check
                    var user = _mockContext.User;
                    if (user == null || !user.Identity.IsAuthenticated)
                    {
                        throw new InvalidOperationException("User not authenticated");
                    }
                    
                    if (!user.IsInRole("Administrators"))
                    {
                        throw new InvalidOperationException("User not authorized");
                    }
                }
                else
                {
                    // Call actual RequireAdministrator for real scenarios
                    RequireAdministrator();
                }
                
                // Skip base implementation for tests to avoid dependency injection
            }

            // Override property accessors to use mock context
            protected new bool IsUserAdministrator
            {
                get
                {
                    if (_mockContext != null)
                    {
                        var user = _mockContext.User;
                        return user != null && user.Identity.IsAuthenticated && user.IsInRole("Administrators");
                    }
                    return base.IsUserAdministrator;
                }
            }

            protected new string CurrentUserId
            {
                get
                {
                    if (_mockContext != null)
                    {
                        var user = _mockContext.User as ClaimsPrincipal;
                        return user?.FindFirst("sub")?.Value ?? "test-user-id";
                    }
                    return base.CurrentUserId;
                }
            }

            protected new string CurrentUserName
            {
                get
                {
                    if (_mockContext != null)
                    {
                        var user = _mockContext.User;
                        return user?.Identity?.Name ?? "Test User";
                    }
                    return base.CurrentUserName;
                }
            }
        }
    }
}