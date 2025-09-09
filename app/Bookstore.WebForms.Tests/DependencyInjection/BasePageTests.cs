using System;
using System.Web;
using System.Web.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Autofac;
using Autofac.Integration.Owin;
using Microsoft.Owin;
using Bookstore.WebForms;

namespace Bookstore.WebForms.Tests.DependencyInjection
{
    [TestClass]
    public class BasePageTests
    {
        private TestBasePage _page;
        private Mock<ILifetimeScope> _mockLifetimeScope;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<IOwinContext> _mockOwinContext;

        [TestInitialize]
        public void Setup()
        {
            _page = new TestBasePage();
            _mockLifetimeScope = new Mock<ILifetimeScope>();
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockOwinContext = new Mock<IOwinContext>();
        }

        [TestMethod]
        public void IsUserAuthenticated_WhenUserIsNull_ReturnsFalse()
        {
            // Arrange
            _page.SetUser(null);

            // Act
            var result = _page.IsUserAuthenticated;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsUserAuthenticated_WhenUserIdentityIsNull_ReturnsFalse()
        {
            // Arrange
            var mockUser = new Mock<System.Security.Principal.IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns((System.Security.Principal.IIdentity)null);
            _page.SetUser(mockUser.Object);

            // Act
            var result = _page.IsUserAuthenticated;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsUserAuthenticated_WhenUserIsNotAuthenticated_ReturnsFalse()
        {
            // Arrange
            var mockIdentity = new Mock<System.Security.Principal.IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(false);
            var mockUser = new Mock<System.Security.Principal.IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            _page.SetUser(mockUser.Object);

            // Act
            var result = _page.IsUserAuthenticated;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsUserAuthenticated_WhenUserIsAuthenticated_ReturnsTrue()
        {
            // Arrange
            var mockIdentity = new Mock<System.Security.Principal.IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(true);
            var mockUser = new Mock<System.Security.Principal.IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            _page.SetUser(mockUser.Object);

            // Act
            var result = _page.IsUserAuthenticated;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CurrentUserName_WhenUserIsAuthenticated_ReturnsUserName()
        {
            // Arrange
            var expectedUserName = "testuser@example.com";
            var mockIdentity = new Mock<System.Security.Principal.IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(true);
            mockIdentity.Setup(i => i.Name).Returns(expectedUserName);
            var mockUser = new Mock<System.Security.Principal.IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            _page.SetUser(mockUser.Object);

            // Act
            var result = _page.CurrentUserName;

            // Assert
            Assert.AreEqual(expectedUserName, result);
        }

        [TestMethod]
        public void CurrentUserName_WhenUserIsNotAuthenticated_ReturnsEmptyString()
        {
            // Arrange
            _page.SetUser(null);

            // Act
            var result = _page.CurrentUserName;

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ShowError_SetsErrorMessageInSession()
        {
            // Arrange
            var errorMessage = "Test error message";
            var mockSession = new Mock<HttpSessionStateBase>();
            _page.SetSession(mockSession.Object);

            // Act
            _page.ShowError(errorMessage);

            // Assert
            mockSession.VerifySet(s => s["ErrorMessage"] = errorMessage, Times.Once);
        }

        [TestMethod]
        public void ShowSuccess_SetsSuccessMessageInSession()
        {
            // Arrange
            var successMessage = "Test success message";
            var mockSession = new Mock<HttpSessionStateBase>();
            _page.SetSession(mockSession.Object);

            // Act
            _page.ShowSuccess(successMessage);

            // Assert
            mockSession.VerifySet(s => s["SuccessMessage"] = successMessage, Times.Once);
        }

        // Test class that inherits from BasePage for testing
        public class TestBasePage : BasePage
        {
            private System.Security.Principal.IPrincipal _testUser;
            private HttpSessionStateBase _testSession;

            public override System.Security.Principal.IPrincipal User => _testUser;

            public void SetUser(System.Security.Principal.IPrincipal user)
            {
                _testUser = user;
            }

            public void SetSession(HttpSessionStateBase session)
            {
                _testSession = session;
            }

            protected override HttpSessionStateBase Session => _testSession;

            // Expose protected methods for testing
            public new bool IsUserAuthenticated => base.IsUserAuthenticated;
            public new string CurrentUserName => base.CurrentUserName;
            public new void ShowError(string message) => base.ShowError(message);
            public new void ShowSuccess(string message) => base.ShowSuccess(message);
        }
    }
}