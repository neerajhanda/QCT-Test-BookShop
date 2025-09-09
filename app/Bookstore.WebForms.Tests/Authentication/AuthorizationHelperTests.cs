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
    public class AuthorizationHelperTests
    {
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockRequest = new Mock<HttpRequestBase>();
            _mockResponse = new Mock<HttpResponseBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
        }

        [TestMethod]
        public void IsAuthenticated_WhenUserIsNull_ReturnsFalse()
        {
            // Arrange
            _mockHttpContext.Setup(c => c.User).Returns((IPrincipal)null);
            HttpContext.Current = _mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            var result = AuthorizationHelper.IsAuthenticated();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsAuthenticated_WhenUserIdentityIsNull_ReturnsFalse()
        {
            // Arrange
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns((IIdentity)null);
            _mockHttpContext.Setup(c => c.User).Returns(mockUser.Object);

            // Act
            var result = AuthorizationHelper.IsAuthenticated();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsAuthenticated_WhenUserIsNotAuthenticated_ReturnsFalse()
        {
            // Arrange
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(false);
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            _mockHttpContext.Setup(c => c.User).Returns(mockUser.Object);

            // Act
            var result = AuthorizationHelper.IsAuthenticated();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsAuthenticated_WhenUserIsAuthenticated_ReturnsTrue()
        {
            // Arrange
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(true);
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            _mockHttpContext.Setup(c => c.User).Returns(mockUser.Object);

            // Act
            var result = AuthorizationHelper.IsAuthenticated();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInRole_WhenUserIsNotAuthenticated_ReturnsFalse()
        {
            // Arrange
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(false);
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            _mockHttpContext.Setup(c => c.User).Returns(mockUser.Object);

            // Act
            var result = AuthorizationHelper.IsInRole("Administrators");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInRole_WhenUserIsInRole_ReturnsTrue()
        {
            // Arrange
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(true);
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            mockUser.Setup(u => u.IsInRole("Administrators")).Returns(true);
            _mockHttpContext.Setup(c => c.User).Returns(mockUser.Object);

            // Act
            var result = AuthorizationHelper.IsInRole("Administrators");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsAdministrator_WhenUserIsInAdministratorsRole_ReturnsTrue()
        {
            // Arrange
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(true);
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            mockUser.Setup(u => u.IsInRole("Administrators")).Returns(true);
            _mockHttpContext.Setup(c => c.User).Returns(mockUser.Object);

            // Act
            var result = AuthorizationHelper.IsAdministrator();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsAdministrator_WhenUserIsInAdminRole_ReturnsTrue()
        {
            // Arrange
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(true);
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);
            mockUser.Setup(u => u.IsInRole("Admin")).Returns(true);
            _mockHttpContext.Setup(c => c.User).Returns(mockUser.Object);

            // Act
            var result = AuthorizationHelper.IsAdministrator();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetUserId_WhenUserHasSubClaim_ReturnsSubClaimValue()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("sub", "test-user-id"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            // Act
            var result = AuthorizationHelper.GetUserId();

            // Assert
            Assert.AreEqual("test-user-id", result);
        }

        [TestMethod]
        public void GetUserDisplayName_WhenUserHasGivenAndFamilyName_ReturnsFullName()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("given_name", "John"));
            identity.AddClaim(new Claim("family_name", "Doe"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            // Act
            var result = AuthorizationHelper.GetUserDisplayName();

            // Assert
            Assert.AreEqual("John Doe", result);
        }

        [TestMethod]
        public void GetLoginUrl_WithReturnUrl_ReturnsCorrectUrl()
        {
            // Act
            var result = AuthorizationHelper.GetLoginUrl("/some-page.aspx");

            // Assert
            Assert.AreEqual("~/Login.aspx?ReturnUrl=%2fsome-page.aspx", result);
        }

        [TestMethod]
        public void GetLoginUrl_WithoutReturnUrl_ReturnsBasicLoginUrl()
        {
            // Act
            var result = AuthorizationHelper.GetLoginUrl();

            // Assert
            Assert.AreEqual("~/Login.aspx", result);
        }

        [TestMethod]
        public void GetLogoutUrl_ReturnsCorrectUrl()
        {
            // Act
            var result = AuthorizationHelper.GetLogoutUrl();

            // Assert
            Assert.AreEqual("~/Login.aspx?action=logout", result);
        }
    }
}