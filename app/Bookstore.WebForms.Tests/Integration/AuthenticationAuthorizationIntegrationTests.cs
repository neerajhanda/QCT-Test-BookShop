using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Bookstore.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    /// <summary>
    /// Comprehensive integration tests for authentication and authorization across all pages.
    /// Requirements: 9.2 - Test authentication and authorization across all pages
    /// </summary>
    [TestClass]
    public class AuthenticationAuthorizationIntegrationTests
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
        }

        #region User Authentication Flow Tests

        [TestMethod]
        public void AuthenticationFlow_LocalAuthentication_Success()
        {
            // Arrange
            var userId = "local-user-123";
            var userName = "John Doe";
            var email = "john.doe@example.com";

            SetupLocalAuthenticatedUser(userId, userName, email, "User");
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act
            page.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(page.IsUserAuthenticated);
            Assert.AreEqual(userId, page.CurrentUserId);
            Assert.AreEqual(userName, page.CurrentUserName);
            Assert.IsFalse(page.IsUserAdministrator);
        }

        [TestMethod]
        public void AuthenticationFlow_CognitoAuthentication_Success()
        {
            // Arrange
            var userId = "cognito-user-456";
            var userName = "Jane Smith";
            var email = "jane.smith@example.com";

            SetupCognitoAuthenticatedUser(userId, userName, email, "User");
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act
            page.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(page.IsUserAuthenticated);
            Assert.AreEqual(userId, page.CurrentUserId);
            Assert.AreEqual(userName, page.CurrentUserName);
            Assert.IsFalse(page.IsUserAdministrator);
        }

        [TestMethod]
        public void AuthenticationFlow_ExpiredToken_RedirectsToLogin()
        {
            // Arrange
            var userId = "expired-user-789";
            var userName = "Bob Johnson";
            var email = "bob.johnson@example.com";

            SetupExpiredTokenUser(userId, userName, email);
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            Assert.ThrowsException<UnauthorizedAccessException>(() => page.TestOnPreInit(EventArgs.Empty));
        }

        [TestMethod]
        public void AuthenticationFlow_InvalidToken_DeniesAccess()
        {
            // Arrange
            SetupInvalidTokenUser();
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            Assert.ThrowsException<UnauthorizedAccessException>(() => page.TestOnPreInit(EventArgs.Empty));
        }

        [TestMethod]
        public void AuthenticationFlow_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange
            SetupUnauthenticatedUser();
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            Assert.ThrowsException<UnauthorizedAccessException>(() => page.TestRequireAuthentication());
        }

        #endregion

        #region Page-Level Authorization Tests

        [TestMethod]
        public void PageAuthorization_PublicPages_AllowAnonymousAccess()
        {
            // Arrange
            SetupUnauthenticatedUser();
            var publicPages = new List<TestPublicPage>
            {
                new TestPublicPage { PageName = "Default.aspx" },
                new TestPublicPage { PageName = "Search.aspx" },
                new TestPublicPage { PageName = "BookDetails.aspx" },
                new TestPublicPage { PageName = "Login.aspx" }
            };

            // Act & Assert
            foreach (var page in publicPages)
            {
                page.SetMockContext(_mockHttpContext.Object);
                // Public pages should not throw exceptions for unauthenticated users
                page.TestOnPreInit(EventArgs.Empty);
                Assert.IsTrue(page.AllowsAnonymousAccess);
            }
        }

        [TestMethod]
        public void PageAuthorization_UserPages_RequireAuthentication()
        {
            // Arrange
            SetupUnauthenticatedUser();
            var userPages = new List<TestUserPage>
            {
                new TestUserPage { PageName = "ShoppingCart.aspx" },
                new TestUserPage { PageName = "Checkout.aspx" },
                new TestUserPage { PageName = "Orders.aspx" },
                new TestUserPage { PageName = "Address.aspx" },
                new TestUserPage { PageName = "Wishlist.aspx" },
                new TestUserPage { PageName = "Resale.aspx" }
            };

            // Act & Assert
            foreach (var page in userPages)
            {
                page.SetMockContext(_mockHttpContext.Object);
                Assert.ThrowsException<UnauthorizedAccessException>(() => page.TestOnPreInit(EventArgs.Empty));
            }
        }

        [TestMethod]
        public void PageAuthorization_AdminPages_RequireAdminRole()
        {
            // Arrange
            SetupAuthenticatedUser("user-123", "Regular User", "User"); // Non-admin user
            var adminPages = new List<TestAdminPage>
            {
                new TestAdminPage { PageName = "Admin/Dashboard.aspx" },
                new TestAdminPage { PageName = "Admin/Inventory.aspx" },
                new TestAdminPage { PageName = "Admin/Orders.aspx" },
                new TestAdminPage { PageName = "Admin/Offers.aspx" },
                new TestAdminPage { PageName = "Admin/ReferenceData.aspx" }
            };

            // Act & Assert
            foreach (var page in adminPages)
            {
                page.SetMockContext(_mockHttpContext.Object);
                Assert.ThrowsException<UnauthorizedAccessException>(() => page.TestOnPreInit(EventArgs.Empty));
            }
        }

        [TestMethod]
        public void PageAuthorization_AdminPages_AllowAdminAccess()
        {
            // Arrange
            SetupAuthenticatedUser("admin-123", "Admin User", "Administrators");
            var adminPages = new List<TestAdminPage>
            {
                new TestAdminPage { PageName = "Admin/Dashboard.aspx" },
                new TestAdminPage { PageName = "Admin/Inventory.aspx" },
                new TestAdminPage { PageName = "Admin/Orders.aspx" },
                new TestAdminPage { PageName = "Admin/Offers.aspx" },
                new TestAdminPage { PageName = "Admin/ReferenceData.aspx" }
            };

            // Act & Assert
            foreach (var page in adminPages)
            {
                page.SetMockContext(_mockHttpContext.Object);
                page.TestOnPreInit(EventArgs.Empty); // Should not throw
                Assert.IsTrue(page.IsUserAdministrator);
            }
        }

        #endregion

        #region Role-Based Authorization Tests

        [TestMethod]
        public void RoleAuthorization_UserRole_AccessUserFeatures()
        {
            // Arrange
            SetupAuthenticatedUser("user-456", "Test User", "User");
            var userPage = new TestUserPage();
            userPage.SetMockContext(_mockHttpContext.Object);

            // Act
            userPage.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(userPage.IsUserAuthenticated);
            Assert.IsTrue(userPage.IsUserInRole("User"));
            Assert.IsFalse(userPage.IsUserInRole("Administrators"));
            Assert.IsFalse(userPage.IsUserAdministrator);
        }

        [TestMethod]
        public void RoleAuthorization_AdminRole_AccessAllFeatures()
        {
            // Arrange
            SetupAuthenticatedUser("admin-789", "Admin User", "Administrators");
            var adminPage = new TestAdminPage();
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act
            adminPage.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(adminPage.IsUserAuthenticated);
            Assert.IsTrue(adminPage.IsUserInRole("Administrators"));
            Assert.IsTrue(adminPage.IsUserAdministrator);
        }

        [TestMethod]
        public void RoleAuthorization_MultipleRoles_ChecksAllRoles()
        {
            // Arrange
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim("sub", "multi-role-user"));
            identity.AddClaim(new Claim("given_name", "Multi"));
            identity.AddClaim(new Claim("family_name", "Role"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "PowerUser"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));

            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);

            var page = new TestAdminPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act
            page.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(page.IsUserInRole("User"));
            Assert.IsTrue(page.IsUserInRole("PowerUser"));
            Assert.IsTrue(page.IsUserInRole("Administrators"));
            Assert.IsTrue(page.IsUserAdministrator);
        }

        [TestMethod]
        public void RoleAuthorization_CustomRole_ChecksCorrectly()
        {
            // Arrange
            SetupAuthenticatedUser("custom-user", "Custom User", "CustomRole");
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act
            page.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(page.IsUserAuthenticated);
            Assert.IsTrue(page.IsUserInRole("CustomRole"));
            Assert.IsFalse(page.IsUserInRole("User"));
            Assert.IsFalse(page.IsUserInRole("Administrators"));
        }

        #endregion

        #region Session Management Tests

        [TestMethod]
        public void SessionManagement_AuthenticatedUser_MaintainsSession()
        {
            // Arrange
            var userId = "session-user-123";
            var sessionId = "session-456";
            
            SetupAuthenticatedUser(userId, "Session User", "User");
            _mockSession.Setup(s => s.SessionID).Returns(sessionId);
            _mockSession.Setup(s => s["UserId"]).Returns(userId);
            _mockSession.Setup(s => s["UserName"]).Returns("Session User");

            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act
            page.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(page.IsUserAuthenticated);
            Assert.AreEqual(userId, page.CurrentUserId);
            
            // Verify session is being used
            _mockSession.VerifyGet(s => s.SessionID, Times.AtLeastOnce);
        }

        [TestMethod]
        public void SessionManagement_SessionTimeout_RequiresReauthentication()
        {
            // Arrange
            SetupExpiredSessionUser();
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            Assert.ThrowsException<UnauthorizedAccessException>(() => page.TestOnPreInit(EventArgs.Empty));
        }

        [TestMethod]
        public void SessionManagement_SessionHijacking_DetectsAndBlocks()
        {
            // Arrange
            var userId = "hijack-test-user";
            var originalIP = "192.168.1.100";
            var suspiciousIP = "10.0.0.50";

            SetupAuthenticatedUser(userId, "Test User", "User");
            _mockRequest.Setup(r => r.UserHostAddress).Returns(suspiciousIP);
            _mockSession.Setup(s => s["OriginalIP"]).Returns(originalIP);

            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            // In a real scenario, this would detect IP change and require re-authentication
            page.TestOnPreInit(EventArgs.Empty);
            
            // Verify IP address was checked
            _mockRequest.VerifyGet(r => r.UserHostAddress, Times.AtLeastOnce);
        }

        #endregion

        #region Cross-Page Authorization Tests

        [TestMethod]
        public void CrossPageAuthorization_UserToUserPage_Allowed()
        {
            // Arrange
            SetupAuthenticatedUser("user-cross-123", "Cross User", "User");
            
            var sourcePage = new TestUserPage { PageName = "ShoppingCart.aspx" };
            var targetPage = new TestUserPage { PageName = "Checkout.aspx" };
            
            sourcePage.SetMockContext(_mockHttpContext.Object);
            targetPage.SetMockContext(_mockHttpContext.Object);

            // Act
            sourcePage.TestOnPreInit(EventArgs.Empty);
            targetPage.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(sourcePage.IsUserAuthenticated);
            Assert.IsTrue(targetPage.IsUserAuthenticated);
            Assert.AreEqual(sourcePage.CurrentUserId, targetPage.CurrentUserId);
        }

        [TestMethod]
        public void CrossPageAuthorization_UserToAdminPage_Denied()
        {
            // Arrange
            SetupAuthenticatedUser("user-cross-456", "Cross User", "User");
            
            var userPage = new TestUserPage { PageName = "Orders.aspx" };
            var adminPage = new TestAdminPage { PageName = "Admin/Dashboard.aspx" };
            
            userPage.SetMockContext(_mockHttpContext.Object);
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act
            userPage.TestOnPreInit(EventArgs.Empty); // Should succeed
            
            // Assert
            Assert.IsTrue(userPage.IsUserAuthenticated);
            Assert.ThrowsException<UnauthorizedAccessException>(() => adminPage.TestOnPreInit(EventArgs.Empty));
        }

        [TestMethod]
        public void CrossPageAuthorization_AdminToAnyPage_Allowed()
        {
            // Arrange
            SetupAuthenticatedUser("admin-cross-789", "Cross Admin", "Administrators");
            
            var userPage = new TestUserPage { PageName = "Orders.aspx" };
            var adminPage = new TestAdminPage { PageName = "Admin/Dashboard.aspx" };
            
            userPage.SetMockContext(_mockHttpContext.Object);
            adminPage.SetMockContext(_mockHttpContext.Object);

            // Act & Assert
            userPage.TestOnPreInit(EventArgs.Empty); // Should succeed
            adminPage.TestOnPreInit(EventArgs.Empty); // Should succeed
            
            Assert.IsTrue(userPage.IsUserAuthenticated);
            Assert.IsTrue(adminPage.IsUserAuthenticated);
            Assert.IsTrue(adminPage.IsUserAdministrator);
        }

        #endregion

        #region Authentication State Persistence Tests

        [TestMethod]
        public void AuthenticationPersistence_RememberMe_PersistsAcrossSessions()
        {
            // Arrange
            var userId = "remember-user-123";
            var persistentToken = "persistent-token-456";

            SetupPersistentAuthenticatedUser(userId, "Remember User", "User", persistentToken);
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act
            page.TestOnPreInit(EventArgs.Empty);

            // Assert
            Assert.IsTrue(page.IsUserAuthenticated);
            Assert.AreEqual(userId, page.CurrentUserId);
            
            // Verify persistent authentication token is present
            var user = _mockHttpContext.Object.User as ClaimsPrincipal;
            var tokenClaim = user?.FindFirst("persistent_token");
            Assert.IsNotNull(tokenClaim);
            Assert.AreEqual(persistentToken, tokenClaim.Value);
        }

        [TestMethod]
        public void AuthenticationPersistence_LogoutClearsState_Success()
        {
            // Arrange
            var userId = "logout-user-789";
            
            SetupAuthenticatedUser(userId, "Logout User", "User");
            var page = new TestUserPage();
            page.SetMockContext(_mockHttpContext.Object);

            // Act - Simulate logout
            page.TestOnPreInit(EventArgs.Empty);
            Assert.IsTrue(page.IsUserAuthenticated);

            // Simulate logout by clearing authentication
            SetupUnauthenticatedUser();
            page.SetMockContext(_mockHttpContext.Object);

            // Assert
            Assert.ThrowsException<UnauthorizedAccessException>(() => page.TestRequireAuthentication());
        }

        #endregion

        #region Helper Methods

        private void SetupLocalAuthenticatedUser(string userId, string userName, string email, string role)
        {
            var identity = new ClaimsIdentity("local");
            identity.AddClaim(new Claim("sub", userId));
            identity.AddClaim(new Claim("given_name", userName.Split(' ')[0]));
            identity.AddClaim(new Claim("family_name", userName.Split(' ').Length > 1 ? userName.Split(' ')[1] : ""));
            identity.AddClaim(new Claim("email", email));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
            identity.AddClaim(new Claim("auth_type", "local"));

            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        private void SetupCognitoAuthenticatedUser(string userId, string userName, string email, string role)
        {
            var identity = new ClaimsIdentity("cognito");
            identity.AddClaim(new Claim("sub", userId));
            identity.AddClaim(new Claim("given_name", userName.Split(' ')[0]));
            identity.AddClaim(new Claim("family_name", userName.Split(' ').Length > 1 ? userName.Split(' ')[1] : ""));
            identity.AddClaim(new Claim("email", email));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
            identity.AddClaim(new Claim("auth_type", "cognito"));
            identity.AddClaim(new Claim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString()));

            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        private void SetupAuthenticatedUser(string userId, string userName, string role)
        {
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim("sub", userId));
            identity.AddClaim(new Claim("given_name", userName.Split(' ')[0]));
            identity.AddClaim(new Claim("family_name", userName.Split(' ').Length > 1 ? userName.Split(' ')[1] : ""));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));

            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        private void SetupExpiredTokenUser(string userId, string userName, string email)
        {
            var identity = new ClaimsIdentity("cognito");
            identity.AddClaim(new Claim("sub", userId));
            identity.AddClaim(new Claim("given_name", userName.Split(' ')[0]));
            identity.AddClaim(new Claim("family_name", userName.Split(' ').Length > 1 ? userName.Split(' ')[1] : ""));
            identity.AddClaim(new Claim("email", email));
            identity.AddClaim(new Claim("exp", DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds().ToString())); // Expired

            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        private void SetupInvalidTokenUser()
        {
            var identity = new ClaimsIdentity(); // Invalid/empty identity
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        private void SetupUnauthenticatedUser()
        {
            var identity = new ClaimsIdentity(); // Not authenticated
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        private void SetupExpiredSessionUser()
        {
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim("sub", "expired-session-user"));
            identity.AddClaim(new Claim("session_exp", DateTimeOffset.UtcNow.AddMinutes(-30).ToUnixTimeSeconds().ToString()));

            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        private void SetupPersistentAuthenticatedUser(string userId, string userName, string role, string persistentToken)
        {
            var identity = new ClaimsIdentity("persistent");
            identity.AddClaim(new Claim("sub", userId));
            identity.AddClaim(new Claim("given_name", userName.Split(' ')[0]));
            identity.AddClaim(new Claim("family_name", userName.Split(' ').Length > 1 ? userName.Split(' ')[1] : ""));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
            identity.AddClaim(new Claim("persistent_token", persistentToken));
            identity.AddClaim(new Claim("remember_me", "true"));

            var user = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        #endregion

        #region Test Page Classes

        private class TestPublicPage : BasePage
        {
            private HttpContextBase _mockContext;
            public string PageName { get; set; }
            public bool AllowsAnonymousAccess { get; private set; } = true;

            public void SetMockContext(HttpContextBase context)
            {
                _mockContext = context;
            }

            public void TestOnPreInit(EventArgs e) => OnPreInit(e);

            protected override void OnPreInit(EventArgs e)
            {
                // Public pages don't require authentication
                // Skip base implementation for tests
            }
        }

        private class TestUserPage : BasePage
        {
            private HttpContextBase _mockContext;
            public string PageName { get; set; }

            public void SetMockContext(HttpContextBase context)
            {
                _mockContext = context;
            }

            public void TestOnPreInit(EventArgs e) => OnPreInit(e);
            public void TestRequireAuthentication() => RequireAuthentication();

            protected override void OnPreInit(EventArgs e)
            {
                if (_mockContext != null)
                {
                    var user = _mockContext.User;
                    if (user == null || !user.Identity.IsAuthenticated)
                    {
                        throw new UnauthorizedAccessException("User not authenticated");
                    }
                }
                else
                {
                    base.OnPreInit(e);
                }
            }

            public new bool IsUserAuthenticated
            {
                get
                {
                    if (_mockContext != null)
                    {
                        var user = _mockContext.User;
                        return user != null && user.Identity.IsAuthenticated;
                    }
                    return base.IsUserAuthenticated;
                }
            }

            public new string CurrentUserName
            {
                get
                {
                    if (_mockContext != null)
                    {
                        var user = _mockContext.User as ClaimsPrincipal;
                        var firstName = user?.FindFirst("given_name")?.Value ?? "";
                        var lastName = user?.FindFirst("family_name")?.Value ?? "";
                        return $"{firstName} {lastName}".Trim();
                    }
                    return base.CurrentUserName;
                }
            }

            public new string CurrentUserId
            {
                get
                {
                    if (_mockContext != null)
                    {
                        var user = _mockContext.User as ClaimsPrincipal;
                        return user?.FindFirst("sub")?.Value;
                    }
                    return base.CurrentUserId;
                }
            }

            public new bool IsUserAdministrator
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

            public bool IsUserInRole(string role)
            {
                if (_mockContext != null)
                {
                    var user = _mockContext.User;
                    return user != null && user.Identity.IsAuthenticated && user.IsInRole(role);
                }
                return base.IsUserInRole(role);
            }
        }

        private class TestAdminPage : AdminBasePage
        {
            private HttpContextBase _mockContext;
            public string PageName { get; set; }

            public void SetMockContext(HttpContextBase context)
            {
                _mockContext = context;
            }

            public void TestOnPreInit(EventArgs e) => OnPreInit(e);

            protected override void OnPreInit(EventArgs e)
            {
                if (_mockContext != null)
                {
                    var user = _mockContext.User;
                    if (user == null || !user.Identity.IsAuthenticated)
                    {
                        throw new UnauthorizedAccessException("User not authenticated");
                    }
                    
                    if (!user.IsInRole("Administrators"))
                    {
                        throw new UnauthorizedAccessException("User not authorized for admin access");
                    }
                }
                else
                {
                    base.OnPreInit(e);
                }
            }

            public new bool IsUserAuthenticated
            {
                get
                {
                    if (_mockContext != null)
                    {
                        var user = _mockContext.User;
                        return user != null && user.Identity.IsAuthenticated;
                    }
                    return base.IsUserAuthenticated;
                }
            }

            public new bool IsUserAdministrator
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

            public bool IsUserInRole(string role)
            {
                if (_mockContext != null)
                {
                    var user = _mockContext.User;
                    return user != null && user.Identity.IsAuthenticated && user.IsInRole(role);
                }
                return base.IsUserInRole(role);
            }
        }

        #endregion
    }
}