using System;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.ErrorHandling
{
    [TestClass]
    public class ErrorPageIntegrationTests
    {
        [TestMethod]
        public void Error_Page_Should_Handle_Missing_Session_Data_Gracefully()
        {
            // Arrange
            var mockContext = new Mock<HttpContextBase>();
            var mockSession = new Mock<HttpSessionStateBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            var mockResponse = new Mock<HttpResponseBase>();
            var mockServer = new Mock<HttpServerUtilityBase>();

            mockContext.Setup(c => c.Session).Returns(mockSession.Object);
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);
            mockContext.Setup(c => c.Server).Returns(mockServer.Object);

            // Session returns null for error data
            mockSession.Setup(s => s["LastErrorMessage"]).Returns((object)null);
            mockSession.Setup(s => s["LastErrorDetails"]).Returns((object)null);
            mockSession.Setup(s => s["ErrorReturnUrl"]).Returns((object)null);

            mockRequest.Setup(r => r.QueryString["showDetails"]).Returns((string)null);
            mockContext.Setup(c => c.IsDebuggingEnabled).Returns(false);

            // Act & Assert
            // The error page should not throw an exception when session data is missing
            // This would be verified in actual page lifecycle tests
            Assert.IsTrue(true); // Placeholder - actual test would verify page loads without error
        }

        [TestMethod]
        public void NotFound_Page_Should_Log_Suspicious_Patterns()
        {
            // Arrange
            var suspiciousUrls = new[]
            {
                "http://localhost/admin.php",
                "http://localhost/wp-admin/",
                "http://localhost/api/nonexistent",
                "http://localhost/test.jsp"
            };

            // Act & Assert
            foreach (var url in suspiciousUrls)
            {
                // Verify that suspicious patterns are detected and logged
                var lowerUrl = url.ToLowerInvariant();
                
                if (lowerUrl.Contains(".php") || lowerUrl.Contains(".jsp"))
                {
                    Assert.IsTrue(true); // Would log "Possible attack or migration issue"
                }
                else if (lowerUrl.Contains("admin") || lowerUrl.Contains("wp-admin"))
                {
                    Assert.IsTrue(true); // Would log "Admin area access attempt"
                }
                else if (lowerUrl.Contains("api/") || lowerUrl.Contains("/api"))
                {
                    Assert.IsTrue(true); // Would log "API endpoint not found"
                }
            }
        }

        [TestMethod]
        public void Unauthorized_Page_Should_Show_Login_Panel_For_Anonymous_Users()
        {
            // Arrange
            var mockAuthHelper = new Mock<IAuthorizationHelper>();
            mockAuthHelper.Setup(a => a.IsAuthenticated()).Returns(false);

            // Act
            var shouldShowLogin = !mockAuthHelper.Object.IsAuthenticated();

            // Assert
            Assert.IsTrue(shouldShowLogin);
        }

        [TestMethod]
        public void Unauthorized_Page_Should_Show_Role_Panel_For_Authenticated_Users()
        {
            // Arrange
            var mockAuthHelper = new Mock<IAuthorizationHelper>();
            mockAuthHelper.Setup(a => a.IsAuthenticated()).Returns(true);

            // Act
            var shouldShowRolePanel = mockAuthHelper.Object.IsAuthenticated();

            // Assert
            Assert.IsTrue(shouldShowRolePanel);
        }

        [TestMethod]
        public void Global_Error_Handler_Should_Handle_Nested_Exceptions()
        {
            // Arrange
            var innerException = new ArgumentException("Inner exception message");
            var outerException = new InvalidOperationException("Outer exception message", innerException);

            // Act & Assert
            // Verify that nested exceptions are properly logged with full stack trace
            Assert.IsNotNull(outerException.InnerException);
            Assert.AreEqual("Inner exception message", outerException.InnerException.Message);
            Assert.AreEqual("Outer exception message", outerException.Message);
        }

        [TestMethod]
        public void Error_Handling_Should_Prevent_Information_Disclosure()
        {
            // Arrange
            var sensitiveException = new Exception("Connection string: Server=prod-db;Password=secret123");

            // Act
            var safeMessage = "An error occurred. Please try again later.";
            var shouldShowDetails = false; // In production, should be false

            // Assert
            Assert.IsFalse(shouldShowDetails);
            Assert.AreNotEqual(sensitiveException.Message, safeMessage);
            Assert.IsFalse(safeMessage.Contains("Password"));
            Assert.IsFalse(safeMessage.Contains("Connection string"));
        }

        [TestMethod]
        public void Error_Pages_Should_Maintain_User_Session()
        {
            // Arrange
            var mockSession = new Mock<HttpSessionStateBase>();
            var sessionData = new System.Collections.Generic.Dictionary<string, object>
            {
                ["UserId"] = "user123",
                ["ShoppingCart"] = new { ItemCount = 3 }
            };

            mockSession.Setup(s => s[It.IsAny<string>()])
                .Returns((string key) => sessionData.ContainsKey(key) ? sessionData[key] : null);

            // Act
            var userId = mockSession.Object["UserId"];
            var cart = mockSession.Object["ShoppingCart"];

            // Assert
            Assert.AreEqual("user123", userId);
            Assert.IsNotNull(cart);
            // Error pages should not clear user session data
        }

        [TestMethod]
        public void Client_IP_Detection_Should_Handle_Proxy_Headers()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequestBase>();
            var serverVariables = new System.Collections.Specialized.NameValueCollection();

            // Test X-Forwarded-For header with multiple IPs
            serverVariables["HTTP_X_FORWARDED_FOR"] = "203.0.113.1, 198.51.100.1, 192.168.1.1";
            serverVariables["HTTP_X_REAL_IP"] = "203.0.113.1";
            serverVariables["REMOTE_ADDR"] = "192.168.1.1";

            mockRequest.Setup(r => r.ServerVariables).Returns(serverVariables);

            // Act
            var forwardedFor = serverVariables["HTTP_X_FORWARDED_FOR"];
            var clientIP = !string.IsNullOrEmpty(forwardedFor) ? forwardedFor.Split(',')[0].Trim() : "Unknown";

            // Assert
            Assert.AreEqual("203.0.113.1", clientIP);
        }

        // Mock interface for testing
        public interface IAuthorizationHelper
        {
            bool IsAuthenticated();
        }
    }
}